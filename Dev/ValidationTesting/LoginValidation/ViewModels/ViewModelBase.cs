using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LoginValidation.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public readonly Dictionary<string, List<string>> Errors = new();

	public bool HasErrors => Errors.Any();

	public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

	public IEnumerable GetErrors(string? propertyName)
	{
		if (string.IsNullOrEmpty(propertyName) || !Errors.ContainsKey(propertyName))
		{
			return Enumerable.Empty<string>();
		}
		else
		{
			return Errors.GetValueOrDefault(propertyName, null);
		}
	}

	public List<Tuple<string, string>> AllErrors =>
		Errors.SelectMany(f => f.Value.Select(s => new Tuple<string, string>(f.Key, s)))
			 .ToList();

	public string GetAllErrorsAsString()
	{
		return string.Join(Environment.NewLine, AllErrors.Select(e => $"{e.Item1}: {e.Item2}"));
	}

	public void AddError(string propertyName, string errorMessage)
	{
		if (!Errors.ContainsKey(propertyName))
		{
			Errors.Add(propertyName, new List<string>());
		}

		Errors[propertyName].Add(errorMessage);
		OnErrorsChanged(propertyName);
	}

	public void ClearErrors(string? propertyName = null)
	{
		if (string.IsNullOrEmpty(propertyName))
		{
			Errors.Clear();
		}
		else
		{
			if (Errors.Remove(propertyName))
			{
				OnErrorsChanged(propertyName);
			}
		}
	}

	public void OnErrorsChanged(string propertyName)
	{
		ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
	}

	protected bool SetProperty<T>(ref T member, T val, [CallerMemberName] string propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(member, val))
		{
			return false;
		}

		member = val;
		OnPropertyChanged(propertyName);
		return true;
	}
}