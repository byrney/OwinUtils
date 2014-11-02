

namespace OwinUtils
{
    using System;
    using RouteDict = System.Collections.Generic.Dictionary<string, object>;
    using System.Threading.Tasks;
    using System.Reflection;
    using System.Linq;
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;


	public class Wrapper
	{
		object callee;
		ParameterInfo[] parameterInfo;
		Type[] parameterTypes;
		MethodInfo method;

		public Wrapper (object callee, string methodName = "Invoke")
		{
			this.callee = callee;
			extractMetadata (callee, methodName);
		}

		private void extractMetadataMatch(object callee, string methodName)
		{
			MethodInfo[] methods = callee.GetType().GetMethods();
			foreach (var method in methods)
			{
				if (method.Name != methodName) {
					continue;
				}
				ParameterInfo[] parameters = method.GetParameters();
				Type[] parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
				this.parameterInfo = parameters;
				this.parameterTypes = parameterTypes;
				this.method = method;
				return;
			}
			throw new ArgumentException ("cannot find invoke method on callee");
		}


		private void extractMetadata(object callee, string methodName)
		{
			MethodInfo method = callee.GetType().GetMethod(methodName);
			ParameterInfo[] parameters = method.GetParameters();
			Type[] parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
            //todo: check hat first arg is an EnvDict
			this.parameterInfo = parameters;
			this.parameterTypes = parameterTypes;
			this.method = method;
			return;
		}

        public Task Invoke(EnvDict env)
        {
            RouteDict routeParams = (RouteDict)env["RouteParams"];
            return InvokeRoute(env, routeParams);
        }

        object tryArgFromDict(ParameterInfo param, RouteDict routeParams)
        {
            var name = param.Name;
            var type = param.ParameterType;
            object dictValue;
            if (routeParams.TryGetValue(name, out dictValue)) {
                return dictValue;
            } else {
                if (param.HasDefaultValue == false) {
                    return null;
                }
            }
            return null;
        }

		public Task InvokeRoute(EnvDict env, RouteDict routeParams)
		{
			object[] args = new object[this.parameterInfo.Length];
			int argIndex = 0;
			foreach (var param in this.parameterInfo) {
                if (argIndex == 0) {
                    args[argIndex] = env;
                } else {
                    args[argIndex] = tryArgFromDict(param, routeParams);
                }
				argIndex += 1;
			}
			return (Task)this.method.Invoke (callee, args);
		}

	}
}

