using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace FirstPrinciples.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo, ICollectErrors
{
	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public readonly Dictionary<string, List<string>> ErrorsByPropertyName = new();

	public bool HasErrors => ErrorsByPropertyName.Any();

	public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

	public IEnumerable GetErrors(string? propertyName)
	{
		if (string.IsNullOrEmpty(propertyName) || !ErrorsByPropertyName.ContainsKey(propertyName))
		{
			return Enumerable.Empty<string>();
		}
		else
		{
			return ErrorsByPropertyName.GetValueOrDefault(propertyName, null);
		}
	}

	public List<Tuple<string, string>> AllErrors =>
		ErrorsByPropertyName.SelectMany(f => f.Value.Select(s => new Tuple<string, string>(f.Key, s)))
			 .ToList();

	public string GetAllErrorsAsString()
	{
		return string.Join(Environment.NewLine, AllErrors.Select(e => $"{e.Item1}: {e.Item2}"));
	}

	public void AddError(string propertyName, string errorMessage)
	{
		if (!ErrorsByPropertyName.ContainsKey(propertyName))
		{
			ErrorsByPropertyName.Add(propertyName, new List<string>());
		}

		ErrorsByPropertyName[propertyName].Add(errorMessage);
		OnErrorsChanged(propertyName);
	}

	public void ClearErrors(string? propertyName = null)
	{
		if (string.IsNullOrEmpty(propertyName))
		{
			ErrorsByPropertyName.Clear();
		}
		else
		{
			if (ErrorsByPropertyName.Remove(propertyName))
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

	public void SetErrors(string? propertyName = null, IEnumerator<string> errors = null)
	{
		throw new NotImplementedException();
	}

	IEnumerable<string> ICollectErrors.GetErrors(string? propertyName)
	{
		throw new NotImplementedException();
	}
}