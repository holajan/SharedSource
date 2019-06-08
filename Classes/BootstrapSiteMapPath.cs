[DefaultProperty("Text")]
[ToolboxData("<{0}:BootstrapSiteMapPath runat=server></{0}:BootstrapSiteMapPath>")]
public class BootstrapSiteMapPath : SiteMapPath
{
    public BootstrapSiteMapPath()
    {
        this.PathSeparator = null;
        this.RenderCurrentNodeAsLink = false;
        ShowToolTips = false;
    }

    protected override void Render(HtmlTextWriter writer)
    {
        var node = this.Provider.CurrentNode;
        var nodes = new Stack<SiteMapNode>();

        for (var currentNode = node; currentNode != null; currentNode = currentNode.ParentNode)
        {
            nodes.Push(currentNode);
        }

        var sb = new System.Text.StringBuilder();

        sb.AppendLine(@"<ul class=""breadcrumb"" " + this.CssClass + ">");

        foreach (var currentNode in nodes)
        {
            if (currentNode.Url == this.Provider.CurrentNode.Url)
            {
                sb.AppendLine(@"<li class=""active"">" + currentNode.Title + "</li>");
            }
            else
            {
                sb.AppendLine(@"<li><a href=""" + currentNode.Url + @""">" + currentNode.Title + "</a></li>");
            }
        }

        sb.AppendLine(@"</ul>");

        writer.Write(sb.ToString());
    }
}