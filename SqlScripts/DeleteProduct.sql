DROP TABLE IF EXISTS VariantsToDelete;
CREATE TEMPORARY TABLE VariantsToDelete AS SELECT Id, ProductId FROM ProductVariant WHERE ProductId = ?;
DELETE FROM SpecificationAttribute WHERE ProductVariantId IN (SELECT Id FROM VariantsToDelete);
DELETE FROM Product WHERE Id IN (SELECT ProductId FROM VariantsToDelete);
DELETE FROM ProductVariant WHERE Id IN (SELECT Id FROM VariantsToDelete);
DROP TABLE VariantsToDelete;
UPDATE AppState SET LastProductsListRefresh = DATE('2000-01-01') WHERE 1=1;