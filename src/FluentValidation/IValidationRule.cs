#region License
// Copyright (c) .NET Foundation and contributors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// The latest version of this file can be found at https://github.com/FluentValidation/FluentValidation
#endregion

namespace FluentValidation {
	using System;
	using System.Collections.Generic;
	using System.Linq.Expressions;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;
	using Internal;
	using Results;
	using Validators;

	public interface ICollectionRule<T, TElement> : IValidationRule<T, TElement> {
		/// <summary>
		/// Filter that should include/exclude items in the collection.
		/// </summary>
		public Func<TElement, bool> Filter { get; set; }

		/// <summary>
		/// Constructs the indexer in the property name associated with the error message.
		/// By default this is "[" + index + "]"
		/// </summary>
		public Func<T, IEnumerable<TElement>, TElement, int, string> IndexBuilder { get; set; }
	}

	internal interface IExecutableValidationRule<T> : IValidationRule<T> {
		void Validate(ValidationContext<T> context);

		Task ValidateAsync(ValidationContext<T> context, CancellationToken cancellation);

		void AddDependentRules(IEnumerable<IExecutableValidationRule<T>> rules);

		List<IExecutableValidationRule<T>> DependentRules { get; }

		AbstractValidator<T> ParentValidator { get; }
	}

	public interface IValidationRule<T, TProperty> : IValidationRule<T> {
		/// <summary>
		/// Cascade mode for this rule.
		/// </summary>
		public CascadeMode CascadeMode { get; set; }

		/// <summary>
		/// Function that will be invoked if any of the validators associated with this rule fail.
		/// </summary>
		public Action<T, IEnumerable<ValidationFailure>> OnFailure { get; set; }

		/// <summary>
		/// Sets the display name for the property.
		/// </summary>
		/// <param name="name">The property's display name</param>
		void SetDisplayName(string name);

		/// <summary>
		/// Sets the display name for the property using a function.
		/// </summary>
		/// <param name="factory">The function for building the display name</param>
		void SetDisplayName(Func<ValidationContext<T>, string> factory);

		/// <summary>
		/// Adds a component to the rule.
		/// </summary>
		/// <param name="component"></param>
		/// <typeparam name="TValidator"></typeparam>
		void Add<TValidator>(RuleComponent<T, TProperty, TValidator> component);

		/// <summary>
		/// The current rule component.
		/// </summary>
		RuleComponent<T,TProperty> Current { get; }

		[Obsolete("The current validator is no longer directly exposed. Access the current component with rule.Current instead. This property will be removed in FluentValidation 11.")]
		RuleComponent<T,TProperty> CurrentValidator { get; }

		/// <summary>
		/// Allows custom creation of an error message
		/// </summary>
		public Func<MessageBuilderContext<T,TProperty>, string> MessageBuilder { get; set; }
	}

	public interface IValidationRule<T> : IValidationRule {
		void ApplyCondition(Func<IValidationContext, bool> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators);

		void ApplyAsyncCondition(Func<IValidationContext, CancellationToken, Task<bool>> predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators);

		void ApplySharedCondition(Func<ValidationContext<T>, bool> condition);

		void ApplySharedAsyncCondition(Func<ValidationContext<T>, CancellationToken, Task<bool>> condition);
	}

	/// <summary>
	/// Defines a rule associated with a property which can have multiple validators.
	/// </summary>
	public interface IValidationRule {
		/// <summary>
		/// The components in this rule.
		/// </summary>
		IEnumerable<IRuleComponent> Components { get; }
		/// <summary>
		/// Name of the rule-set to which this rule belongs.
		/// </summary>
		string[] RuleSets { get; set; }

		/// <summary>
		/// Gets the display name for the property.
		/// </summary>
		/// <param name="context">Current context</param>
		/// <returns>Display name</returns>
		string GetDisplayName(IValidationContext context);

		/// <summary>
		/// Returns the property name for the property being validated.
		/// Returns null if it is not a property being validated (eg a method call)
		/// </summary>
		public string PropertyName { get; set; }

		/// <summary>
		/// Property associated with this rule.
		/// </summary>
		public MemberInfo Member { get; }

		/// <summary>
		/// Type of the property being validated
		/// </summary>
		public Type TypeToValidate { get; }

		/// <summary>
		/// Whether the rule has a condition defined.
		/// </summary>
		bool HasCondition { get; }

		/// <summary>
		/// Whether the rule has an async condition defined.
		/// </summary>
		bool HasAsyncCondition { get; }

		/// <summary>
		/// Expression that was used to create the rule.
		/// </summary>
		public LambdaExpression Expression { get; }
	}
}
