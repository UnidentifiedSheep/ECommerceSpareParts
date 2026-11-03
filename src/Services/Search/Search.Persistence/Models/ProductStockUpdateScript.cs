namespace Search.Persistence.Models;

internal static class ProductStockUpdateScript
{
	public const string Source = """
		if (ctx._source.stockSnapshotVersion != null && ctx._source.stockSnapshotVersion >= params.version) {
		    ctx.op = 'none'; return;
		}
		if (ctx._source.stocks == null) { ctx._source.stocks = new HashMap(); }
		if (ctx._source.stockVersions == null) { ctx._source.stockVersions = new HashMap(); }
		if (params.isSnapshot) {
		    def stocks = new HashMap(params.stocks);
		    def versions = new HashMap();
		    for (def entry : ctx._source.stockVersions.entrySet()) {
		        if (entry.getValue() > params.version) {
		            def key = entry.getKey();
		            versions.put(key, entry.getValue());
		            if (ctx._source.stocks.containsKey(key)) { stocks.put(key, ctx._source.stocks[key]); }
		            else { stocks.remove(key); }
		        }
		    }
		    ctx._source.stocks = stocks;
		    ctx._source.stockVersions = versions;
		    ctx._source.stockSnapshotVersion = params.version;
		} else {
		    boolean changed = false;
		    for (def entry : params.stocks.entrySet()) {
		        def key = entry.getKey();
		        def previous = ctx._source.stockVersions[key];
		        if (previous != null && previous >= params.version) { continue; }
		        if (entry.getValue() > 0) { ctx._source.stocks[key] = entry.getValue(); }
		        else { ctx._source.stocks.remove(key); }
		        ctx._source.stockVersions[key] = params.version;
		        changed = true;
		    }
		    if (!changed) { ctx.op = 'none'; return; }
		}
		if (ctx._source.stockSnapshotVersion != null) {
		    long total = 0L;
		    for (def quantity : ctx._source.stocks.values()) { total += ((Number)quantity).longValue(); }
		    ctx._source.stock = total;
		} else { ctx._source.stock = null; }
		""";
}
