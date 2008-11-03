// DynamicObject.cs
//

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Scripting.Actions {

    /// <summary>
    /// Provides a simple class that can be inherited from to create an object with dynamic behavior
    /// at runtime.  Subclasses can override the various action methods (GetMember, SetMember, Call, etc...)
    /// to provide custom behavior that will be invoked at runtime.  
    /// 
    /// If a method is not overridden then the DynamicObject does not directly support that behavior and 
    /// the call site will determine how the action should be performed.
    /// </summary>
    public abstract class DynamicObject : IDynamicObject {

        private StandardActionKinds _supportedActions;

        /// <summary>
        /// Enables derived types to create a new instance of DynamicObject.  DynamicObject instances
        /// cannot be directly instantiated because they have no implementation of dynamic behavior.
        /// </summary>
        /// <param name="supportedActions">The list of actions that are supported.</param>
        protected DynamicObject(StandardActionKinds supportedActions) {
            _supportedActions = supportedActions;
        }

        protected StandardActionKinds SupportedActions {
            get {
                return _supportedActions;
            }
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of calling a member
        /// in the expando.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        protected virtual object Call(CallAction action, params object[] args) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of converting the
        /// Dynamic object to another type.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        protected virtual object Convert(ConvertAction action) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of creating an instance
        /// of the Dynamic object.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        protected virtual object Create(CreateAction action, params object[] args) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of deleting a member.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        protected virtual bool DeleteMember(DeleteMemberAction action) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of getting a member.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        protected virtual object GetMember(GetMemberAction action) {
            throw new NotSupportedException();
        }

        protected virtual MetaObject GetMetaObject(Expression parameter) {
            return new MetaDynamicObject(parameter, this);
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of invoking the
        /// Dynamic object.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        protected virtual object Invoke(InvokeAction action, params object[] args) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of performing
        /// the operation.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        protected virtual object Operation(OperationAction action, params object[] args) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class provides the non-Meta implementation of setting a member.
        /// 
        /// When not overridden the call site requesting the action determines the behavior.
        /// </summary>
        protected virtual void SetMember(SetMemberAction action, object value) {
            throw new NotSupportedException();
        }

        #region Implementation of IDynamicObject
        /// <summary>
        /// Can be overridden in the derived class.  The provided
        /// MetaObject will dispatch to the Dynamic virtual methods.  The
        /// object can be encapsulated inside of another MetaObject to
        /// provide custom behavior for individual actions.
        /// </summary>
        MetaObject IDynamicObject.GetMetaObject(Expression parameter) {
            return GetMetaObject(parameter);
        }
        #endregion


        protected class MetaDynamicObject : MetaObject {

            public MetaDynamicObject(Expression expression, DynamicObject value)
                : base(expression, Restrictions.Empty, value) {
            }

            private new DynamicObject Value {
                get {
                    return (DynamicObject)base.Value;
                }
            }

            public override MetaObject Call(CallAction action, MetaObject[] args) {
                if (ImplementsActions(StandardActionKinds.Call)) {
                    return CallMethodNAry(action, args, "Call");
                }

                return base.Call(action, args);
            }

            /// <summary>
            /// Helper method for generating a MetaObject which calls a specific method declared on
            /// Dynamic w/ one additional parameter.
            /// </summary>
            private MetaObject CallMethodBinary(SetMemberAction action, MetaObject arg, string methodName) {
                return new MetaObject(
                    Expression.Call(
                        GetLimitedSelf(),
                        typeof(DynamicObject).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance),
                        Expression.Constant(action),
                        arg.Expression
                    ),
                    GetRestrictions()
                );
            }

            /// <summary>
            /// Helper method for generating a MetaObject which calls a specific method on Dynamic w/ the
            /// meta object array as the params.
            /// </summary>
            private MetaObject CallMethodNAry(MetaAction action, MetaObject[] args, string methodName) {
                return new MetaObject(
                    Expression.Call(
                        GetLimitedSelf(),
                        typeof(DynamicObject).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance),
                        ReplaceSelfWithAction(action, args)
                    ),
                    GetRestrictions()
                );
            }

            /// <summary>
            /// Helper method for generating a MetaObject which calls a specific method on Dynamic
            /// w/o any additional parameters.
            /// </summary>
            private MetaObject CallMethodUnary(MetaAction action, string methodName) {
                return new MetaObject(
                    Expression.Call(
                        GetLimitedSelf(),
                        typeof(DynamicObject).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance),
                        Expression.Constant(action)
                    ),
                    GetRestrictions()
                );
            }

            public override MetaObject Create(CreateAction action, MetaObject[] args) {
                if (ImplementsActions(StandardActionKinds.Create)) {
                    return CallMethodNAry(action, args, "Create");
                }

                return base.Create(action, args);
            }

            public override MetaObject Convert(ConvertAction action, MetaObject[] args) {
                if (ImplementsActions(StandardActionKinds.Convert)) {
                    return CallMethodUnary(action, "Convert");
                }

                return base.Convert(action, args);
            }

            public override MetaObject DeleteMember(DeleteMemberAction action, MetaObject[] args) {
                if (ImplementsActions(StandardActionKinds.DeleteMember)) {
                    return CallMethodUnary(action, "DeleteMember");
                }

                return base.DeleteMember(action, args);
            }

            public override MetaObject GetMember(GetMemberAction action, MetaObject[] args) {
                if (ImplementsActions(StandardActionKinds.GetMember)) {
                    return CallMethodUnary(action, "GetMember");
                }

                return base.GetMember(action, args);
            }

            /// <summary>
            /// Returns our Expression converted to our known LimitType
            /// </summary>
            private Expression GetLimitedSelf() {
                return Expression.ConvertHelper(Expression, LimitType);
            }

            /// <summary>
            /// Returns a Restrictions object which includes our current restrictions merged
            /// with a restriction limiting our type
            /// </summary>
            private Restrictions GetRestrictions() {
                Debug.Assert(Restrictions == Restrictions.Empty, "We don't merge, restrictions are always empty");

                return Restrictions.TypeRestriction(Expression, LimitType);
            }

            private bool ImplementsActions(StandardActionKinds actions) {
                return (Value.SupportedActions & actions) != 0;
            }

            public override MetaObject Invoke(InvokeAction action, MetaObject[] args) {
                if (ImplementsActions(StandardActionKinds.Invoke)) {
                    return CallMethodNAry(action, args, "Invoke");
                }

                return base.Invoke(action, args);
            }

            public override MetaObject Operation(OperationAction action, MetaObject[] args) {
                if (ImplementsActions(StandardActionKinds.Operation)) {
                    return CallMethodNAry(action, args, "Operation");
                }

                return base.Operation(action, args);
            }

            /// <summary>
            /// Helper which returns an Expression array corresponding to the arguments minus 
            /// the DynamicObject instance (arg0) plus the MetaAction constant.
            /// </summary>
            private static Expression[] ReplaceSelfWithAction(MetaAction action, MetaObject[] args) {
                Expression[] paramArgs = new Expression[args.Length - 1];

                for (int i = 0; i < paramArgs.Length; i++) {
                    paramArgs[i] = Expression.ConvertHelper(args[i + 1].Expression, typeof(object));
                }

                return new Expression[] { 
                    Expression.Constant(action), 
                    Expression.NewArrayInit(typeof(object), paramArgs)
                };
            }

            public override MetaObject SetMember(SetMemberAction action, MetaObject[] args) {
                if (ImplementsActions(StandardActionKinds.SetMember)) {
                    return CallMethodBinary(action, args[1], "SetMember");
                }

                return base.SetMember(action, args);
            }
        }
    }
}
