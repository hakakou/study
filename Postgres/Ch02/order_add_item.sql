CREATE OR REPLACE FUNCTION sales.order_add_item(
  customer_id_param INT, product_id_param INT, quantity_param INT)  
RETURNS TABLE (
  order_id UUID, prod_id INT, quantity INT, prod_price DECIMAL) AS $$
DECLARE
    pending_order_id UUID;  
BEGIN
    SELECT id INTO pending_order_id  
    FROM sales.orders
    WHERE customer_id = customer_id_param
      AND status = 'pending'
    LIMIT 1;

    IF pending_order_id IS NULL THEN  
        INSERT INTO sales.orders (customer_id, status)
        VALUES (customer_id_param, 'pending')
        RETURNING id INTO pending_order_id;
    END IF;

    MERGE INTO sales.order_items AS oi  
    USING (SELECT id, price FROM products.catalog WHERE id = product_id_param) AS prod
    ON oi.product_id = prod.id AND oi.order_id = pending_order_id
    WHEN MATCHED THEN
        UPDATE SET quantity = quantity_param
    WHEN NOT MATCHED THEN
        INSERT (order_id, product_id, quantity, price)
        VALUES (pending_order_id, prod.id, quantity_param, prod.price);

    RETURN QUERY  
    SELECT oi.order_id, oi.product_id, oi.quantity, oi.price as prod_price
    FROM sales.order_items as oi
    WHERE oi.order_id = pending_order_id;
END;
$$ LANGUAGE plpgsql;