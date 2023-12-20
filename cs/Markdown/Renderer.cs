using Markdown.Tags;

namespace Markdown;

public class Renderer
{
    private Tag previousTag;
    private Stack<Tag>? stack;
    private string closingSomeTags = string.Empty;

    public List<Token> HandleTokens(List<Token> tokenList)
    {
        stack = new Stack<Tag>();

        foreach (var token in tokenList)
            switch (token.Type)
            {
                case TokenType.Text:
                    break;
                case TokenType.Tag when token.Tag.IsPaired:
                    HandlePairedTag(token.Tag);
                    break;
                case TokenType.Tag:
                    var tag = token.Tag;
                    if (tag.Status == TagStatus.Block) continue;
                    tag.TagContent = tag.ReplacementForOpeningTag;
                    closingSomeTags = tag.ReplacementForClosingTag + closingSomeTags;
                    break;
                case TokenType.Escape:
                    token.Content = "";
                    break;
                case TokenType.LineBreaker:
                    token.Content = closingSomeTags + token.Content;
                    closingSomeTags = string.Empty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        return tokenList;
    }

    private void HandlePairedTag(Tag tag)
    {
        if (tag.Status == TagStatus.Block) return;
        if (tag.Status == TagStatus.Closing && previousTag == null) return;
        if (tag.Status == TagStatus.Opening || previousTag == null ||
            (previousTag.TagType != tag.TagType && previousTag.Status != TagStatus.Closing))
        {
            previousTag = tag;
            tag.Status = TagStatus.Opening;
            stack.Push(tag);
            return;
        }

        stack.TryPeek(out var tokenPeek);
        if (tokenPeek == null) return;
        if (tokenPeek.TagType == tag.TagType)
            ClosePairedTag(tag);
    }

    private void ClosePairedTag(Tag tag)
    {
        var tokenPeek = stack.Pop();
        tag.Status = TagStatus.Closing;
        tokenPeek.TagContent = tag.TagContent = tag.ReplacementForOpeningTag;
        tag.TagContent = tag.ReplacementForClosingTag;
        previousTag = tag;
    }
}