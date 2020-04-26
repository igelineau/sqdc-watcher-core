DROP TABLE IF EXISTS VariantsToDelete;
CREATE TEMPORARY TABLE VariantsToDelete AS SELECT Id, ProductId FROM ProductVariants WHERE ProductId = '694144002510-P';
DELETE FROM SpecificationAttribute WHERE ProductVariantId IN (SELECT Id FROM VariantsToDelete);
DELETE FROM Products WHERE Id IN (SELECT Id FROM VariantsToDelete);
DELETE FROM ProductVariants WHERE Id IN (SELECT Id FROM VariantsToDelete);
DROP TABLE VariantsToDelete;

--UPDATE AppState SET LastProductsListRefresh = DATE('2000-01-01') WHERE 1=1;