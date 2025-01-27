namespace LoginValidation.ViewModels;

public interface ICollectErrors
{
	public void AddError(string propertyName, string errorMessage);
	public void ClearErrors(string? propertyName = null);
	public void SetErrors(string? propertyName = null, IEnumerator<string> errors = null);
	public IEnumerable<string> GetErrors(string? propertyName);
}
