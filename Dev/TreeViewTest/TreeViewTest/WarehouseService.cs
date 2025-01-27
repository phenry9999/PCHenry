using System.Collections.ObjectModel;

namespace TreeViewTest;

public class WarehouseService
{
	public static ObservableCollection<WarehouseItem> GetWarehouseItems()
	{
		ObservableCollection<WarehouseItem> data = new ObservableCollection<WarehouseItem>();

		WarehouseItem drinks = new WarehouseItem("Drinks", 35);
		drinks.Items.Add(new WarehouseItem("Water", 10));
		WarehouseItem tea = new WarehouseItem("Tea", 20);
		tea.Items.Add(new WarehouseItem("Black", 10));
		tea.Items.Add(new WarehouseItem("Green", 10));
		drinks.Items.Add(tea);
		drinks.Items.Add(new WarehouseItem("Coffee", 5));
		data.Add(drinks);
		WarehouseItem vegetables = new WarehouseItem("Vegetables", 75);
		vegetables.Items.Add(new WarehouseItem("Tomato", 40));
		vegetables.Items.Add(new WarehouseItem("Carrot", 25));
		vegetables.Items.Add(new WarehouseItem("Onion", 10));
		data.Add(vegetables);
		WarehouseItem fruits = new WarehouseItem("Fruits", 55);
		fruits.Items.Add(new WarehouseItem("Cherry", 30));
		fruits.Items.Add(new WarehouseItem("Apple", 20));
		fruits.Items.Add(new WarehouseItem("Melon", 5));
		data.Add(fruits);

		return data;
	}
}