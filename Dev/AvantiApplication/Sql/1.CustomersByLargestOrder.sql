use Avanti

--columns matching what is in the question
--these are the columns at the top of the question, this is directly from the "requirement", but maybe this is being a bit too picky?
select o.CustomerID, o.OrderID, o.OrderDate, o.TotalDollars, o.SalesPersonID
from Orders o join
(
	select o.CustomerID, max(o.TotalDollars) LargestOrder from orders o 
	group by o.CustomerID
) as LargestCustomerOrders on o.CustomerID = LargestCustomerOrders.CustomerID and LargestCustomerOrders.LargestOrder = o.TotalDollars
order by o.CustomerID


--columns matching the answer to match
--After realizing the delta in the column requirements, I would like to discuss with the PO about what is really needed for the customer requirements
select o.ID, o.CustomerID, o.OrderDate, o.TotalDollars, o.SalesPersonID
from Orders o join
(
	select o.CustomerID, max(o.TotalDollars) LargestOrder from orders o 
	group by o.CustomerID
) as LargestCustomerOrders on o.CustomerID = LargestCustomerOrders.CustomerID and LargestCustomerOrders.LargestOrder = o.TotalDollars
order by o.CustomerID
