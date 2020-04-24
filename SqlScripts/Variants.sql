SELECT pv.ProductVariantId AS VariantId, p.ProductId AS ProductId, p.Title, pv.GramEquivalent, pv.InStock FROM ProductVariant pv
INNER JOIN Product p ON pv.ProductId = p.ProductId
ORDER BY pv.ProductId