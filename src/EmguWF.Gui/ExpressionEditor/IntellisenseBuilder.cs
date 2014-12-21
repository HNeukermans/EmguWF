using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmguWF.ExpressionEditor
{
    /// <summary>
    ///     This class is used to load the intellisense for the project.
    /// </summary>
    public static class IntellisenseBuilder
    {
        private static readonly string[] IgnoreNamespaces = {"XamlGeneratedNamespace", "Microsoft", "Internal"};

        /// <summary>
        ///     Load the default set of intellisense from all referenced assemblies.
        /// </summary>
        /// <returns></returns>
        public static Task<IntellisenseEntry> LoadAsync()
        {
            Assembly workflowAsm = Assembly.GetExecutingAssembly();
            Assembly[] asmList = workflowAsm.GetReferencedAssemblies().Select(Assembly.Load).ToArray();
            return LoadAsync(asmList, IgnoreNamespaces);
        }

        /// <summary>
        ///     Load intellisense for the given assembly list
        /// </summary>
        /// <param name="assembliesToLoad"></param>
        /// <param name="ignoreNamespaces"></param>
        /// <returns></returns>
        public static Task<IntellisenseEntry> LoadAsync(IEnumerable<Assembly> assembliesToLoad,
            IEnumerable<string> ignoreNamespaces)
        {
            return Task.Run(() => LoadInternal(assembliesToLoad, ignoreNamespaces));
        }

        /// <summary>
        ///     Loads intellisense for the given assemblies and namespaces.
        /// </summary>
        /// <param name="asmList"></param>
        /// <param name="ignoreNamespaces"></param>
        /// <returns></returns>
        private static IntellisenseEntry LoadInternal(IEnumerable<Assembly> asmList,
            IEnumerable<string> ignoreNamespaces)
        {
            if (ignoreNamespaces == null)
                ignoreNamespaces = Enumerable.Empty<string>();

            List<Type> typeList = asmList.SelectMany(a => a.GetTypes().Where(
                x => x.IsPublic && x.IsVisible && x.Namespace != null
                     && !ignoreNamespaces.Any(s => x.Namespace.ToLowerInvariant().Contains(s.ToLowerInvariant()))))
                .ToList();

            var data = new IntellisenseEntry();
            foreach (Type type in typeList)
            {
                AddNode(data, type.Namespace, true);
                AddTypeNode(data, type);
            }
            SortNodes(data);

            return data;
        }

        /// <summary>
        ///     Searches for a specific set of nodes.
        /// </summary>
        /// <param name="targetNodes"></param>
        /// <param name="namePath"></param>
        /// <returns></returns>
        private static IntellisenseEntry SearchNodes(IntellisenseEntry targetNodes, string namePath)
        {
            string targetPath = namePath.Split('.')[0];
            IntellisenseEntry foundNode =
                targetNodes.Children.FirstOrDefault(
                    str => String.Equals(str.Name, targetPath, StringComparison.CurrentCultureIgnoreCase));
            if (foundNode == null)
                return targetNodes;

            string nextPath = namePath.Substring(targetPath.Length, namePath.Length - targetPath.Length);
            if (nextPath.StartsWith("."))
                nextPath = nextPath.Substring(1, nextPath.Length - 1);
            return string.IsNullOrWhiteSpace(nextPath)
                ? foundNode
                : SearchNodes(foundNode, nextPath);
        }

        /// <summary>
        ///     Adds a node into the intellisense graph.
        /// </summary>
        /// <param name="targetNodes"></param>
        /// <param name="namePath"></param>
        private static void AddNode(IntellisenseEntry targetNodes, string namePath)
        {
            IntellisenseEntry existsNode;
            string[] targetPath = namePath.Split('.');
            List<IntellisenseEntry> validNode =
                targetNodes.Children.Where(
                    x => String.Equals(x.Name, targetPath[0], StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (validNode.Count > 0)
            {
                existsNode = validNode[0];
            }
            else
            {
                existsNode = new IntellisenseEntry
                             {
                                 Name = targetPath[0],
                                 SimpleName = targetPath[0],
                                 Type = IntellisenseEntryType.Namespace,
                                 Parent = targetNodes,
                                 Description = string.Format("Namespace {0}", targetPath[0]),
                             };
                targetNodes.AddNode(existsNode);
            }

            string nextPath = namePath.Substring(targetPath[0].Length, namePath.Length - targetPath[0].Length);
            if (nextPath.StartsWith("."))
                nextPath = nextPath.Substring(1, nextPath.Length - 1);
            if (!string.IsNullOrWhiteSpace(nextPath))
                AddNode(existsNode, nextPath);
        }

        /// <summary>
        ///     Adds a node into the intellisense graph
        /// </summary>
        /// <param name="targetNodes"></param>
        /// <param name="namePath"></param>
        /// <param name="isNamespace"></param>
        private static void AddNode(IntellisenseEntry targetNodes, string namePath, bool isNamespace)
        {
            IntellisenseEntry existsNode;
            string[] targetPath = namePath.Split('.');
            List<IntellisenseEntry> validNode =
                targetNodes.Children.Where(
                    x => String.Equals(x.Name, targetPath[0], StringComparison.InvariantCultureIgnoreCase)).ToList();
            if (validNode.Count > 0)
            {
                existsNode = validNode[0];
            }
            else
            {
                existsNode = new IntellisenseEntry
                             {
                                 Name = targetPath[0],
                                 SimpleName = targetPath[0],
                                 Type =
                                     (isNamespace) ? IntellisenseEntryType.Namespace : IntellisenseEntryType.Primitive,
                                 Parent = targetNodes,
                                 Description = (isNamespace) ? string.Format("Namespace {0}", targetPath[0]) : "",
                             };
                targetNodes.AddNode(existsNode);
            }

            if (isNamespace)
            {
                string nextPath = namePath.Substring(targetPath[0].Length, namePath.Length - targetPath[0].Length);
                if (nextPath.StartsWith("."))
                    nextPath = nextPath.Substring(1, nextPath.Length - 1);
                if (!string.IsNullOrWhiteSpace(nextPath))
                    AddNode(existsNode, nextPath);
            }
        }

        /// <summary>
        ///     Adds a type node into the intellisense graph
        /// </summary>
        /// <param name="targetNodes"></param>
        /// <param name="target"></param>
        /// <param name="allowAbstract"></param>
        internal static void AddTypeNode(IntellisenseEntry targetNodes, Type target, bool allowAbstract = false)
        {
            if ((!target.IsAbstract || allowAbstract) && target.IsVisible)
            {
                string namePath = target.Namespace;
                string name = target.Name;
                IntellisenseEntry nodes = SearchNodes(targetNodes, namePath);
                string str = name;

                var node = new IntellisenseEntry
                           {
                               Name = name,
                               SimpleName = name,
                               Parent = nodes,
                               SystemType = target
                           };

                if (target.IsGenericType)
                {
                    node.Type = IntellisenseEntryType.Class;
                    if (name.Contains("`"))
                    {
                        str = name.Substring(0, name.LastIndexOf("`", StringComparison.Ordinal));
                        node.SimpleName = str;
                    }

                    var builder = new StringBuilder();
                    int num = 0;
                    foreach (Type type in target.GetGenericArguments())
                    {
                        if (num > 0)
                        {
                            builder.Append(", ");
                        }
                        builder.Append(type.Name);
                        num++;
                    }
                    str = str + "(" + builder + ")";
                    node.Name = str;
                    node.Description = string.Format("Class {0}", node.SimpleName);
                }
                else if (target.IsClass)
                {
                    node.Type = IntellisenseEntryType.Class;
                    node.Description = string.Format("Class {0}", node.SimpleName);
                }
                else if (target.IsEnum)
                {
                    node.Type = IntellisenseEntryType.Enum;
                    node.Description = string.Format("Enum {0}", node.SimpleName);
                }
                else if (target.IsInterface)
                {
                    node.Type = IntellisenseEntryType.Interface;
                    node.Description = string.Format("Interface {0}", node.SimpleName);
                }
                else if (target.IsPrimitive)
                {
                    node.Type = IntellisenseEntryType.Primitive;
                    node.Description = string.Format("{0}", node.SimpleName);
                }
                else if (target.IsValueType)
                {
                    node.Type = IntellisenseEntryType.ValueType;
                    node.Description = string.Format("{0}", node.SimpleName);
                }
                else
                {
                    return;
                }

                if (nodes == null)
                {
                    targetNodes.AddNode(node);
                }
                else
                {
                    nodes.AddNode(node);
                }

                AddMethodNode(node, target);
                AddPropertyNode(node, target);
                AddFieldNode(node, target);
                AddEventNode(node, target);
                AddNestedTypeNode(node, target);
            }
        }

        /// <summary>
        /// Add a method node into the intellisense graph
        /// </summary>
        /// <param name="targetNodes"></param>
        /// <param name="target"></param>
        private static void AddMethodNode(IntellisenseEntry targetNodes, Type target)
        {
            foreach (
                MethodInfo val in target.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                // Ignore property implementation methods
                if (val.IsSpecialName)
                    continue;

                var memberNodes = new IntellisenseEntry
                                  {
                                      Name = val.Name,
                                      SimpleName = val.Name,
                                      Type = IntellisenseEntryType.Method,
                                      Parent = targetNodes,
                                      Description = CreateMethodDescription(val),
                                  };
                targetNodes.AddNode(memberNodes);
            }
        }

        private static void AddNestedTypeNode(IntellisenseEntry targetNodes, Type target)
        {
            foreach (
                Type val in target.GetNestedTypes(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                var memberNodes = new IntellisenseEntry
                                  {
                                      Name = val.Name,
                                      SimpleName = val.Name,
                                      Type = IntellisenseEntryType.Method,
                                      Parent = targetNodes,
                                  };
                targetNodes.AddNode(memberNodes);
            }
        }

        /// <summary>
        /// Add a field node into the intellisense graph
        /// </summary>
        /// <param name="targetNodes"></param>
        /// <param name="target"></param>
        private static void AddFieldNode(IntellisenseEntry targetNodes, Type target)
        {
            foreach (
                FieldInfo val in target.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                var memberNodes = new IntellisenseEntry
                                  {
                                      Name = val.Name,
                                      SimpleName = val.Name,
                                      Type = IntellisenseEntryType.Field,
                                      Parent = targetNodes,
                                      Description = CreateFieldDescription(val),
                                  };
                targetNodes.AddNode(memberNodes);
            }
        }

        /// <summary>
        /// Add an event node into the intellisense graph
        /// </summary>
        /// <param name="targetNodes"></param>
        /// <param name="target"></param>
        private static void AddEventNode(IntellisenseEntry targetNodes, Type target)
        {
            foreach (
                EventInfo val in target.GetEvents(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                var memberNodes = new IntellisenseEntry
                                  {
                                      Name = val.Name,
                                      SimpleName = val.Name,
                                      Type = IntellisenseEntryType.Event,
                                      Parent = targetNodes,
                                      Description = CreateEventDescription(val),
                                  };
                targetNodes.AddNode(memberNodes);
            }
        }

        /// <summary>
        /// Create the text for an event entry
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static string CreateEventDescription(EventInfo target)
        {
            var builder = new StringBuilder();
            builder.Append(target.Name);
            if (target.EventHandlerType != null)
            {
                builder.Append("As " + target.EventHandlerType.Name);
                if (target.EventHandlerType.IsGenericType)
                {
                    builder.Append(CreateGenericParameter(target.EventHandlerType));
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Create the text for a field entry
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static string CreateFieldDescription(FieldInfo target)
        {
            var builder = new StringBuilder();
            if (target.IsPublic)
                builder.Append("Public ");
            if (target.IsPrivate)
                builder.Append("Private ");
            if (target.IsStatic)
                builder.Append("Shared ");

            builder.Append(target.Name);
            builder.Append("() ");
            if (target.FieldType != null)
            {
                builder.Append("As " + target.FieldType.Name);
                builder.Append(CreateGenericParameter(target.FieldType));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Create the text for a property entry
        /// </summary>
        /// <param name="targetNodes"></param>
        /// <param name="target"></param>
        private static void AddPropertyNode(IntellisenseEntry targetNodes, Type target)
        {
            foreach (
                PropertyInfo val in
                    target.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                var memberNodes = new IntellisenseEntry
                                  {
                                      Name = val.Name,
                                      SimpleName = val.Name,
                                      Type = IntellisenseEntryType.Property,
                                      Parent = targetNodes,
                                      Description = CreatePropertyDescription(val),
                                  };
                targetNodes.AddNode(memberNodes);
            }
        }

        /// <summary>
        /// Create the text for a property entry
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static string CreatePropertyDescription(PropertyInfo target)
        {
            var builder = new StringBuilder();
            if (!target.CanRead || !target.CanWrite)
            {
                if (target.CanRead)
                {
                    builder.Append("ReadOnly ");
                }
                else
                {
                    builder.Append("WriteOnly ");
                }
            }
            builder.Append("Property " + target.Name + " As " + target.PropertyType.Name);
            builder.Append(CreateGenericParameter(target.PropertyType));
            return builder.ToString();
        }

        /// <summary>
        /// Create the text for a method entry
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static string CreateMethodDescription(MethodInfo target)
        {
            var builder = new StringBuilder();
            if (target.IsPublic)
                builder.Append("Public ");
            if (target.IsFamily)
                builder.Append("Protected ");
            if (target.IsAssembly)
                builder.Append("Friend ");
            if (target.IsPrivate)
                builder.Append("Private ");
            if (target.IsAbstract)
                builder.Append("MustOverride ");
            if (target.IsVirtual && !target.IsFinal)
                builder.Append("Overridable ");
            if (target.IsStatic)
                builder.Append("Shared ");
            builder.Append(!(target.ReturnType == typeof (void)) ? "Function " : "Sub ");
            builder.Append(target.Name);
            builder.Append(CreateGenericParameter(target));
            builder.Append("(");
            int num = 0;

            foreach (ParameterInfo info in target.GetParameters())
            {
                if (num > 0)
                    builder.Append(", ");
                if (info.IsOptional)
                    builder.Append("Optional ");
                builder.Append(info.IsOut ? "ByRef " : "ByVal ");
                builder.Append(info.Name + " As " + info.ParameterType.Name);
                builder.Append(CreateGenericParameter(info.ParameterType));
                if (!(info.DefaultValue is DBNull))
                {
                    if (info.DefaultValue == null)
                        builder.Append(" = Nothing");
                    else
                        builder.Append(" = " + info.DefaultValue);
                }
                num++;
            }
            builder.Append(") ");

            if (target.ReturnType != null)
            {
                builder.Append("As " + target.ReturnType.Name);
                builder.Append(CreateGenericParameter(target.ReturnType));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Create the generic entry
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static string CreateGenericParameter(MethodInfo target)
        {
            var builder = new StringBuilder();
            if (target.IsGenericMethod)
            {
                builder.Append("(Of ");
                int num = 0;
                foreach (Type type in target.GetGenericArguments())
                {
                    if (num > 0)
                        builder.Append(", ");
                    builder.Append(type.Name);
                    num++;
                }
                builder.Append(")");
            }
            return builder.ToString();
        }

        /// <summary>
        /// Create the text for a generic entry
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private static string CreateGenericParameter(Type target)
        {
            var builder = new StringBuilder();
            if (target.IsGenericType)
            {
                builder.Append("(Of ");
                int num = 0;
                foreach (Type type in target.GetGenericArguments())
                {
                    if (num > 0)
                        builder.Append(", ");
                    builder.Append(type.Name);
                    num++;
                }
                builder.Append(")");
            }
            return builder.ToString();
        }

        /// <summary>
        /// Sorts the nodes
        /// </summary>
        /// <param name="targetNodes"></param>
        private static void SortNodes(IntellisenseEntry targetNodes)
        {
            targetNodes.Children.Sort(new ComparerName());
            foreach (IntellisenseEntry node in targetNodes.Children)
            {
                SortNodes(node);
            }
        }

        /// <summary>
        /// Comparer used to sort the intellisense entry nodes
        /// </summary>
        internal class ComparerName : IComparer<IntellisenseEntry>
        {
            public int Compare(IntellisenseEntry x, IntellisenseEntry y)
            {
                return x == null
                    ? (y == null ? 0 : -1)
                    : (y == null
                        ? 1
                        : String.Compare(x.Name, y.Name, StringComparison.Ordinal));
            }
        }
    }
}