create or replace function warehouse.restock(p_location_id INT, p_sku TEXT, p_qty_to_add INT)
returns int
as $$

declare product_id_var int;
declare result_var int;
begin
  select id into product_id_var
  from warehouse.products where sku=p_sku
  limit 1;

RAISE NOTICE '1) %', product_id_var;

merge into warehouse.stock as wp
using (select unit_cost from warehouse.products where id=product_id_var) as pr
on wp.product_id = product_id_var and wp.location_id=p_location_id
WHEN MATCHED THEN
 UPDATE set quantity = quantity + p_qty_to_add, last_cost = pr.unit_cost
WHEN NOT MATCHED THEN
 INSERT  (location_id, product_id, quantity, last_cost)
 VALUES (p_location_id, product_id_var, p_qty_to_add, pr.unit_cost);

insert into warehouse.restock_log (    
    location_id,    product_id    ,    quantity_added,logged_at)
	values
	(p_location_id, product_id_var, p_qty_to_add, now() 
);

select quantity into result_var
from warehouse.stock 
where location_Id = p_location_id and product_id=product_id_var 
limit 1;

return result_var;

end;
$$ language plpgsql;

