--I'm making the assumption I don't need to show the sql to create the tables (provided) nor the insert statements?

/*
query from QueryBuilder to help understand pk\fk rels
SELECT        ClientFile.ID, ClientFile.ClientName, Orders.ID AS Expr1, Orders.OrderID, Orders.CustomerID, Orders.OrderDate, Orders.TotalDollars, OrderItems.ID AS Expr2, OrderItems.OrderID AS Expr3, OrderItems.ItemID, 
                         OrderItems.Quantity, Items.ID AS Expr4, Items.ItemID AS Expr5, Items.ItemName
FROM            ClientFile INNER JOIN
                         Orders ON ClientFile.ID = Orders.ID INNER JOIN
                         OrderItems ON Orders.OrderID = OrderItems.OrderID INNER JOIN
                         Items ON OrderItems.ItemID = Items.ItemID
*/


--specification specifically states after Feb 1st, 2015, I'm assuming this is not inclusive and therefore using > and not >=, I would certainly ask the PO if this assumption was indeed correct
--there is a specific call out for items and quantities, but no requirement for order details, I would ask PO about this potential oversight?
select /***/ --I would use this for sql creation\debugging
	CustomerOrdersAfterFeb2015.ClientName, i.ItemName, oi.Quantity	--these are the specific cols asked in the requirement
from 
(
	select customers.ID as CustomerId, customers.ClientName, Orders.OrderID as OrderId
	from ClientFile Customers join Orders on Customers.ID = orders.CustomerID
	where orders.OrderDate > '2015-02-01'
) CustomerOrdersAfterFeb2015 join Orders o on o.CustomerID = CustomerOrdersAfterFeb2015.CustomerId and CustomerOrdersAfterFeb2015.OrderId = o.OrderID
join OrderItems oi on o.OrderID = oi.OrderID
join Items i on oi.ItemID = i.ItemID
order by 1	--trying to show I know how to sort by column number in addition to column name\alias, I do this alot when doing adhoc\support qrys