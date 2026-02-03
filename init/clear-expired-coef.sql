SELECT cron.schedule(
               'delete-expired-article-coefs',
               '0 3 * * *',
               $$DELETE FROM article_coefficients
      WHERE valid_till < NOW()$$
       );