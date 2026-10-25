namespace Main.Persistence.DatabaseObjects;

public static class UserSearchSql
{
	public static class V1
	{
		public const string CreateFunctions = """
			CREATE OR REPLACE FUNCTION auth.user_search_vector(
			    name text, surname text, login text, emails text, phones text, description text)
			RETURNS tsvector
			LANGUAGE sql IMMUTABLE PARALLEL SAFE
			AS $function$
			    SELECT
			        setweight(to_tsvector('simple',
			            replace(lower(concat_ws(' ', name, surname, login)), 'ё', 'е')), 'A')
			        || setweight(to_tsvector('simple',
			            replace(lower(concat_ws(' ', emails, replace(emails, '@', ' '),
			                translate(emails, '@._+-', '     '), phones)), 'ё', 'е')), 'B')
			        || setweight(to_tsvector('simple',
			            replace(lower(coalesce(description, '')), 'ё', 'е')), 'D');
			$function$;

			CREATE OR REPLACE FUNCTION auth.user_search_query(search_term text)
			RETURNS tsquery
			LANGUAGE sql IMMUTABLE PARALLEL SAFE
			AS $function$
			    SELECT coalesce(string_agg(
			        quote_literal(lexeme) || CASE WHEN length(lexeme) >= 3 THEN ':*' ELSE '' END,
			        ' & '), '')::tsquery
			    FROM unnest(to_tsvector('simple',
			        replace(lower(coalesce(search_term, '')), 'ё', 'е'))) AS token(lexeme, positions, weights);
			$function$;

			CREATE OR REPLACE FUNCTION auth.refresh_user_search(target_user_id uuid)
			RETURNS void
			LANGUAGE plpgsql
			AS $function$
			BEGIN
			    -- Serialize changes from different source tables for the same user.
			    -- NO KEY UPDATE remains compatible with foreign-key KEY SHARE locks.
			    PERFORM 1 FROM auth.users WHERE id = target_user_id FOR NO KEY UPDATE;
			    IF NOT FOUND THEN
			        RETURN;
			    END IF;

			    INSERT INTO auth.user_search (user_id, search_vector)
			    SELECT u.id, auth.user_search_vector(
			        info.name, info.surname, u.normalized_user_name,
			        (SELECT string_agg(email.normalized_email, ' ' ORDER BY email.normalized_email)
			         FROM auth.user_emails email WHERE email.user_id = u.id),
			        (SELECT string_agg(phone.normalized_phone, ' ' ORDER BY phone.normalized_phone)
			         FROM auth.user_phones phone WHERE phone.user_id = u.id),
			        info.description)
			    FROM auth.users u
			    LEFT JOIN auth.user_info info ON info.user_id = u.id
			    WHERE u.id = target_user_id
			    ON CONFLICT (user_id) DO UPDATE
			        SET search_vector = EXCLUDED.search_vector
			        WHERE user_search.search_vector IS DISTINCT FROM EXCLUDED.search_vector;
			END;
			$function$;

			CREATE OR REPLACE FUNCTION auth.refresh_user_search_from_user()
			RETURNS trigger
			LANGUAGE plpgsql
			AS $function$
			BEGIN
			    PERFORM auth.refresh_user_search(NEW.id);
			    RETURN NEW;
			END;
			$function$;

			CREATE OR REPLACE FUNCTION auth.refresh_user_search_from_related()
			RETURNS trigger
			LANGUAGE plpgsql
			AS $function$
			BEGIN
			    IF TG_OP = 'DELETE' THEN
			        PERFORM auth.refresh_user_search(OLD.user_id);
			        RETURN OLD;
			    END IF;

			    IF TG_OP = 'UPDATE' AND OLD.user_id IS DISTINCT FROM NEW.user_id THEN
			        PERFORM auth.refresh_user_search(least(OLD.user_id, NEW.user_id));
			        PERFORM auth.refresh_user_search(greatest(OLD.user_id, NEW.user_id));
			    ELSE
			        PERFORM auth.refresh_user_search(NEW.user_id);
			    END IF;
			    RETURN NEW;
			END;
			$function$;
			""";

		public const string CreateTriggers = """
			CREATE OR REPLACE TRIGGER user_search_user_changed
			AFTER INSERT OR UPDATE OF user_name, normalized_user_name ON auth.users
			FOR EACH ROW EXECUTE FUNCTION auth.refresh_user_search_from_user();

			CREATE OR REPLACE TRIGGER user_search_info_insert_delete
			AFTER INSERT OR DELETE ON auth.user_info
			FOR EACH ROW EXECUTE FUNCTION auth.refresh_user_search_from_related();

			CREATE OR REPLACE TRIGGER user_search_info_updated
			AFTER UPDATE OF name, surname, description, user_id ON auth.user_info
			FOR EACH ROW
			WHEN ((OLD.name, OLD.surname, OLD.description, OLD.user_id)
			    IS DISTINCT FROM (NEW.name, NEW.surname, NEW.description, NEW.user_id))
			EXECUTE FUNCTION auth.refresh_user_search_from_related();

			CREATE OR REPLACE TRIGGER user_search_email_insert_delete
			AFTER INSERT OR DELETE ON auth.user_emails
			FOR EACH ROW EXECUTE FUNCTION auth.refresh_user_search_from_related();

			CREATE OR REPLACE TRIGGER user_search_email_updated
			AFTER UPDATE OF normalized_email, user_id ON auth.user_emails
			FOR EACH ROW
			WHEN ((OLD.normalized_email, OLD.user_id) IS DISTINCT FROM (NEW.normalized_email, NEW.user_id))
			EXECUTE FUNCTION auth.refresh_user_search_from_related();

			CREATE OR REPLACE TRIGGER user_search_phone_insert_delete
			AFTER INSERT OR DELETE ON auth.user_phones
			FOR EACH ROW EXECUTE FUNCTION auth.refresh_user_search_from_related();

			CREATE OR REPLACE TRIGGER user_search_phone_updated
			AFTER UPDATE OF normalized_phone, user_id ON auth.user_phones
			FOR EACH ROW
			WHEN ((OLD.normalized_phone, OLD.user_id) IS DISTINCT FROM (NEW.normalized_phone, NEW.user_id))
			EXECUTE FUNCTION auth.refresh_user_search_from_related();
			""";

		public const string Backfill = """
			WITH emails AS (
			    SELECT user_id, string_agg(normalized_email, ' ' ORDER BY normalized_email) AS text
			    FROM auth.user_emails GROUP BY user_id
			), phones AS (
			    SELECT user_id, string_agg(normalized_phone, ' ' ORDER BY normalized_phone) AS text
			    FROM auth.user_phones GROUP BY user_id
			)
			INSERT INTO auth.user_search (user_id, search_vector)
			SELECT u.id, auth.user_search_vector(
			    info.name, info.surname, u.normalized_user_name, emails.text, phones.text, info.description)
			FROM auth.users u
			LEFT JOIN auth.user_info info ON info.user_id = u.id
			LEFT JOIN emails ON emails.user_id = u.id
			LEFT JOIN phones ON phones.user_id = u.id
			ON CONFLICT (user_id) DO UPDATE SET search_vector = EXCLUDED.search_vector;
			""";

		public const string DropTriggers = """
			DROP TRIGGER user_search_phone_updated ON auth.user_phones;
			DROP TRIGGER user_search_phone_insert_delete ON auth.user_phones;
			DROP TRIGGER user_search_email_updated ON auth.user_emails;
			DROP TRIGGER user_search_email_insert_delete ON auth.user_emails;
			DROP TRIGGER user_search_info_updated ON auth.user_info;
			DROP TRIGGER user_search_info_insert_delete ON auth.user_info;
			DROP TRIGGER user_search_user_changed ON auth.users;
			""";

		public const string DropFunctions = """
			DROP FUNCTION auth.refresh_user_search_from_related();
			DROP FUNCTION auth.refresh_user_search_from_user();
			DROP FUNCTION auth.refresh_user_search(uuid);
			DROP FUNCTION auth.user_search_query(text);
			DROP FUNCTION auth.user_search_vector(text, text, text, text, text, text);
			""";
	}
}
