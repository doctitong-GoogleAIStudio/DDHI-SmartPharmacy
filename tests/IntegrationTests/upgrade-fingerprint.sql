SELECT count(*) FROM "Medicines";
SELECT md5(coalesce(string_agg((to_jsonb(m)-'ImportedBy'-'DistributedBy')::text,'' ORDER BY "Id"),'')) FROM "Medicines" m;
SELECT count(*) FROM "AspNetUsers";
SELECT count(*) FROM "StockMovements";
SELECT sum("Quantity") FROM "InventoryBalances";
SELECT count(*) FROM "AuditEvents";

SELECT count(*) FROM "OperationKeys";
SELECT md5(coalesce(string_agg("Key"||"Actor"||"PayloadHash"||"Result",'' ORDER BY "Key"),'')) FROM "OperationKeys";
