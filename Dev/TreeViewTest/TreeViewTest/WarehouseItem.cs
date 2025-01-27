using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeViewTest;
public class WarehouseItem
{
	public string Name { get; set; }
	public int Count { get; set; }
	public ObservableCollection<WarehouseItem> Items { get; set; }

	public WarehouseItem(string name, int count)
	{
		Name = name;
		Count = count;
		Items = new ObservableCollection<WarehouseItem>();
	}
}
