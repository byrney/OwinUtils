

using Microsoft.Owin;

namespace OwinUtils
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using EnvDict = System.Collections.Generic.IDictionary<string, object>;
    using RouteDict = System.Collections.Generic.Dictionary<string, object>;
    using System.ComponentModel;


	class Wrapper
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

        public Wrapper (Delegate callee)
        {
            this.callee = callee;
            extractMetadata (callee, "Invoke");
        }

        static void validateParameters(ParameterInfo[] parameters, Type[] types)
        {
            if(parameters.Length < 1) {
                throw new ArgumentException("Method must have at least 1 parameter (envDict)");
            }
            var firstArgType = types[0];
            if(firstArgType != typeof(EnvDict) && firstArgType != typeof(IOwinContext)) {
                throw new ArgumentException("First parameter of method must be envDict/IOwinContext parameter (envDict)");
            }
        }

	    static MethodInfo extractMethodInfo(Type type, string methodName)
	    {
	        try {
	            return type.GetMethod(methodName);
	        }
	        catch (Exception e) {
	            return null;
	        }
	    }

		private void extractMetadata(object callee, string methodName)
		{
			MethodInfo method = extractMethodInfo(callee.GetType(), methodName);
		    if (method == null) {
		        throw new ArgumentException("Failed to find method on calle: ", callee.GetType().ToString() + "." + methodName);
		    }
			ParameterInfo[] parameters = method.GetParameters();
			Type[] parameterTypes = parameters.Select(p => p.ParameterType).ToArray();
            validateParameters(parameters, parameterTypes);
			this.parameterInfo = parameters;
			this.parameterTypes = parameterTypes;
			this.method = method;
			return;
		}

        static object convertTypes(object input, Type type)
        {
            if (input == null || type.IsAssignableFrom(input.GetType())) {
                return input;
            }
            var cv = TypeDescriptor.GetConverter(type);
            return cv.ConvertFrom(input);
        }

        object tryArgFromDict(ParameterInfo param, RouteDict routeParams)
        {
            var name = param.Name;
            var type = param.ParameterType;
            object dictValue;
            if (routeParams.TryGetValue(name, out dictValue)) {
                return convertTypes(dictValue, type);
            } else {
                if (param.HasDefaultValue == false) {
                    return null;
                }
            }
            return null;    
        }

	    static object convertEnv(EnvDict env, Type type)
	    {
            if (typeof(IOwinContext) == type)
            {
                return new OwinContext(env);
            }
            else
            {
               return env;
            }
	    }

        public Task Invoke(EnvDict env)
        {
            RouteDict routeParams = RouteParams.GetDict(env);
            return InvokeRoute(env, routeParams);
        }

	    public Task InvokeRoute(EnvDict env, RouteDict routeParams)
	    {
	        var numArgs = this.parameterInfo.Length;
	        var args = new object[numArgs];
	        for (int i = 0; i < numArgs; i++) {
	            if (i == 0) {
	                args[i] = convertEnv(env, this.parameterTypes[i]);
	            }
	            else {
	                args[i] = tryArgFromDict(this.parameterInfo[i], routeParams);
	            }
	        }
	        try {
	            Task invokeTask = (Task) this.method.Invoke(callee, args);
	            if (invokeTask.IsFaulted) {
	                throw invokeTask.Exception.InnerException;
	            }
	            return invokeTask;
	        }
	        catch (System.Reflection.TargetInvocationException en) {
	            throw en.InnerException;
	        }
	    }

	}
}

