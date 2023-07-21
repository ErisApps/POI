namespace POI.ThirdParty.ScoreSaber.Exceptions;

public class QueryParameterValidationException : Exception
{
	public string QueryParameter { get; }

	public QueryParameterValidationException(string queryParameter)
	{
		QueryParameter = queryParameter;
	}
}