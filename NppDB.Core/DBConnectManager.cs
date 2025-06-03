using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using NppDB.Comm;

namespace NppDB.Core
{
    public class DbServerManager 
    {
        private readonly List<IDbConnect> _dbConnects = new List<IDbConnect>();

        private DbServerManager()
        {
            LoadConnectTypes();
        }
        
        public void Register(IDbConnect dbConnect)
        {
            if (_dbConnects.Any(x => x.Title.Equals(dbConnect.Title))) 
                throw new ApplicationException("Connection Name exists!");

            _dbConnects.Add(dbConnect);
        }

        public void Unregister(IDbConnect dbConnect)
        {
            _dbConnects.Remove(dbConnect);
        }

        public void Refresh()
        {
            _dbConnects.AsParallel().ForAll(x =>{try { x.Refresh(); }
                catch
                {
                }
            });
        }

        public IEnumerable<IDbConnect> Connections => _dbConnects;

        public void SaveToXml(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(path);

            var xdoc = new XmlDocument();
            XmlNode xconnects = xdoc.CreateElement("connects");
            xdoc.AppendChild(xconnects);

            foreach (var connect in _dbConnects)
            {
                var sw = new StringWriter(new StringBuilder());
                var serializer = new XmlSerializer(connect.GetType(), GetXmlOver());
                serializer.Serialize(sw, connect);
                sw.Flush();
                var xd = new XmlDocument();
                xd.LoadXml(sw.ToString());
                var xcnn = xdoc.ImportNode(xd.DocumentElement, true);
                xcnn.Attributes.RemoveAll();
                xconnects.AppendChild(xcnn);
            }
            xdoc.Save(path);
        }

        private XmlAttributeOverrides GetXmlOver()
        {
            var xmlOver = new XmlAttributeOverrides();
            foreach (var prop in typeof(TreeNode).GetProperties())
            {
                xmlOver.Add(typeof(TreeNode), prop.Name, new XmlAttributes { XmlIgnore = true });
            }
            return xmlOver;
        }

        public void LoadFromXml(string path)
        {
            
            if (!File.Exists(path)) throw new ApplicationException("file not exists : " + path);

            var xdoc = new XmlDocument();
            xdoc.Load(path);
            
            try
            {
                var conns = new List<IDbConnect>();
                foreach (var conn in from dbTyp in _dbTypes from XmlNode node in xdoc.SelectNodes(@"//connects/" + dbTyp.ConnectType.Name) let serializer = new XmlSerializer(dbTyp.ConnectType, GetXmlOver()) select serializer.Deserialize(new StringReader(node.OuterXml)) as IDbConnect)
                {
                    if (NppCommandHost != null && conn is INppDBCommandClient client)
                        client.SetCommandHost(NppCommandHost);
                    if (conn != null)
                    {
                        conn.CommandHost = NppCommandHost;
                    }
                    conns.Add(conn);
                }

                _dbConnects.Clear();
                _dbConnects.AddRange(conns);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    MessageBox.Show(ex.InnerException.Message + "\n" + ex.InnerException.StackTrace,
                        (ex.InnerException != null).ToString());
            }
        }

        public INppDbCommandHost NppCommandHost { get; set; }

        public IDbConnect CreateConnect(DatabaseType databaseType)
        {
            var connect = databaseType.ConnectType.Assembly.CreateInstance(databaseType.ConnectType.FullName ?? string.Empty) as IDbConnect;
            if (NppCommandHost != null && connect is INppDBCommandClient client) client.SetCommandHost(NppCommandHost);
            if (connect != null)
            {
                connect.CommandHost = NppCommandHost;
            }
            return connect;
        }

        public IEnumerable<DatabaseType> GetDatabaseTypes()
        {
            return _dbTypes;
        }

        private readonly List<DatabaseType> _dbTypes = new List<DatabaseType>();
        private void LoadConnectTypes()
        {
            var dir = Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath));
            if (dir == null) return;
            foreach (var filePath in Directory.GetFiles(dir, "*.dll"))
            {
                Assembly assem;
                try
                {
                    assem = Assembly.LoadFrom(filePath);
                }
                catch (Exception e)
                {
                    MessageBox.Show($@"err FilePath: {filePath}
Error: {e}");
                    continue;
                }

                foreach (var typ in assem.GetTypes())
                {
                    if (!typ.IsClass) continue;

                    foreach (var attr in typ.GetCustomAttributes(typeof(ConnectAttr), false))
                    {
                        if (!(attr is ConnectAttr cnattr)) continue;

                        _dbTypes.Add(new DatabaseType { Conn = cnattr, ConnectType = typ });
                    }
                }
            }
        }

        private static DbServerManager _svrMan;
        public static DbServerManager Instance => _svrMan ?? (_svrMan = new DbServerManager());
    }

    public class DatabaseType
    {
        public string Id => Conn.Id;
        private string Title => Conn.Title;
        internal ConnectAttr Conn { get; set; }
        internal Type ConnectType { get; set; }
        
        public override string ToString()
        {
            return Title;
        }
    }

}
