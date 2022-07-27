namespace Validot.Validation.Scopes.Builders
{
    using System;

    using Validot.Specification.Commands;

    internal class RuleCommandScopeBuilder<T> : ICommandScopeBuilder
    {
        private readonly ErrorBuilder _errorsBuilder = new ErrorBuilder();

        private readonly RuleCommand<T> _ruleCommand;

        private string _path;

        private Predicate<T> _executionCondition;

        public RuleCommandScopeBuilder(RuleCommand<T> ruleCommand)
        {
            ThrowHelper.NullArgument(ruleCommand, nameof(ruleCommand));

            _ruleCommand = ruleCommand;

            if (_ruleCommand.Message != null)
            {
                _errorsBuilder = new ErrorBuilder(_ruleCommand.Message, _ruleCommand.Args);
            }
        }

        public ICommandScope Build(IScopeBuilderContext context)
        {
            ThrowHelper.NullArgument(context, nameof(context));

            var ruleCommandScope = new RuleCommandScope<T>
            {
                IsValid = _ruleCommand.Predicate,
                Path = _path,
                ExecutionCondition = _executionCondition,
                ErrorMode = _errorsBuilder.Mode
            };

            if (_errorsBuilder.IsEmpty)
            {
                ruleCommandScope.ErrorId = context.DefaultErrorId;
            }
            else
            {
                var error = _errorsBuilder.Build();

                ruleCommandScope.ErrorId = context.RegisterError(error);
            }

            return ruleCommandScope;
        }

        public bool TryAdd(ICommand command)
        {
            if (command is WithConditionCommand<T> withConditionCommand)
            {
                _executionCondition = withConditionCommand.ExecutionCondition;

                return true;
            }

            if (command is WithPathCommand withPathCommand)
            {
                _path = withPathCommand.Path;

                return true;
            }

            return _errorsBuilder.TryAdd(command);
        }
    }
}
