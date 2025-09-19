SELECT 
    t.TransactionId,
    t.BuyerId,
    t.SellerId,
    t.TransactionStatus,
    t.TokenAmount,
    i.ItemTitle,
    u.UserName as SellerName,
    t.CreatedAt
FROM transactions t
JOIN items i ON t.ItemId = i.ItemId
JOIN app_user u ON t.SellerId = u.user_id
ORDER BY t.CreatedAt DESC;