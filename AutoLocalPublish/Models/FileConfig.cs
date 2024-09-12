using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AutoLocalPublish.Models
{
    public class FileAttr
    {
        public string Key { get; set; }
        public string OpType { get; set; }
    }
  
    public class FileConfig : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            List<FileAttr> confgs = new List<FileAttr>();
            foreach (XmlNode childNode in section.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element && childNode.Name == "add")
                {
                    var dbconfig = new FileAttr();
                    string Key = childNode.Attributes["Key"]?.Value;
                    string OpType = childNode.Attributes["OpType"]?.Value;
                  
                    confgs.Add(new FileAttr
                    {
                        Key = Key,
                        OpType = OpType
                    });
                }
            }
            return confgs;
        }
    }
}
