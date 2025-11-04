using System.ComponentModel.DataAnnotations;

namespace HintQuestions.Domain.Models;

public class HintQuestion
{
	[Key]
	[Required]
	public int Id { get; set; }

	[Required(ErrorMessage = "Question is a required field")]
	public string Question { get; set; }

	public List<HintAnswer> HintAnswers { get; set; }
}

public class HintAnswer
{
	[Key]
	[Required]
	public int Id { get; set; }

	[Required(ErrorMessage = "HintQuestionId is a required field")]
	public int HintQuestionId { get; set; }

	[Required(ErrorMessage = "Answer is a required field")]
	public string Answer { get; set; }

	public HintQuestion HintQuestion { get; set; }
}