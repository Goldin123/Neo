namespace Neo.Application.DTOs;

public class PagedPostDto
{
    public int PostId { get; set; }
    public string? PostTitle { get; set; }
    public string? PostContent { get; set; }
    public DateTime PostCreated { get; set; }
    public bool IsPostFlagged { get; set; }
    public string? FlaggedReason { get; set; }
    public PostUserDto? CreatedUser { get; set; }
    public List<PostTagDto>? Tags { get; set; }
    public List<PostCommentDto>? Comments { get; set; }
    public List<PostLikeDto>? Likes { get; set; }
    public PostSummaryDto? Summary { get; set; }
}