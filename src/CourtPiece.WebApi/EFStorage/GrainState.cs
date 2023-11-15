using System.ComponentModel.DataAnnotations;

public class GrainState
{
    [Key]
    [StringLength(128)]
    public string Id { get; set; }

    public string JsonValue { get; set; }

    public DateTime CreateDateTime { get; set; }
}
