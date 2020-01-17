SELECT pv.Id AS VariantId, p.Id AS ProductId, p.Title, pv.GramEquivalent, pv.InStock FROM ProductVariant pv
INNER JOIN Product p ON pv.ProductId = p.Id
ORDER BY pv.ProductId