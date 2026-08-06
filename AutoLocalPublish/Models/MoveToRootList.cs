using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AutoLocalPublish.Models
{

    public class MoveToRootList : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            List<string> confgs = new List<string>();
            foreach (XmlNode childNode in section.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element && childNode.Name == "add")
                {
                    var dbconfig = new FileAttr();
                    string Key = childNode.Attributes["Key"]?.Value;
                 
                    confgs.Add(Key);
                }
            }
            return confgs;
        }
    }

}
