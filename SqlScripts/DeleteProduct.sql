DROP TABLE IF EXISTS VariantsToDelete;
CREATE TEMPORARY TABLE VariantsToDelete AS SELECT ProductVariantId, ProductId FROM ProductVariant WHERE ProductId = ?;
DELETE FROM SpecificationAttribute WHERE ProductVariantId IN (SELECT SpecificationAttributeId FROM VariantsToDelete);
DELETE FROM Product WHERE ProductId IN (SELECT ProductId FROM VariantsToDelete);
DELETE FROM ProductVariant WHERE ProductVariantId IN (SELECT ProductVariantId FROM VariantsToDelete);
DROP TABLE VariantsToDelete;

--UPDATE AppState SET LastProductsListRefresh = DATE('2000-01-01') WHERE 1=1;