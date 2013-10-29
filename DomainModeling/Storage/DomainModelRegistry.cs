using System;
using System.Collections.Generic;
using System.Linq;
using DomainModeling.Meta;
using Konstruktor;

namespace DomainModeling.Storage
{
	[DefaultImplementation]
	sealed class DomainModelRegistry : IDomainModelRegistry
	{
		readonly IDomainModel _model;

		public readonly Dictionary<Type, MetaType> DomainTypes;
		public readonly Dictionary<Type, MetaType> DestructiveEventTypes;
		public readonly Dictionary<string, MetaType> DomainTypesByName;
		public readonly ILookup<Type, GuidAccessor> GuidAccessorsByType;

		public DomainModelRegistry(IDomainModel model)
		{
			_model = model;

			DomainTypes = makeDomainTypes();
			DestructiveEventTypes = makeDestructiveEventTypes();
			DomainTypesByName = makeDomainTypesByName();
			GuidAccessorsByType = makeGuidAccessorsByType();
		}

		public IEnumerable<MetaType> TopologicallySorted
		{
			get { return _model.MetaTypes; }
		}

		public IEnumerable<Type> TopologicallySortedDomainTypes
		{
			get { return TopologicallySorted.Select(mt => mt.DomainType); }
		}

		public MetaType typeOf<TypeT>()
		{
			return DomainTypes[typeof(TypeT)];
		}

		public MetaType byName(string name)
		{
			return DomainTypesByName[name];
		}

		public MetaType byDestructiveEvent(IDestructiveDomainEvent destructiveEvent)
		{
			return DestructiveEventTypes[destructiveEvent.GetType()];
		}

		public IDestructiveDomainEvent createDestructiveEventByReference(Reference reference)
		{
			return DomainTypes[reference.Type].createDestructiveEvent(reference.Id);
		}

		Dictionary<Type, MetaType> makeDomainTypes()
		{
			return _model.MetaTypes.ToDictionary(mt => mt.DomainType, mt => mt);
		}

		Dictionary<Type, MetaType> makeDestructiveEventTypes()
		{
			return _model.MetaTypes.Where(mt => mt.DestructiveEventType_ != null)
				.ToDictionary(mt => mt.DestructiveEventType_, mt => mt);
		}

		Dictionary<string, MetaType> makeDomainTypesByName()
		{
			return _model.MetaTypes.ToDictionary(mt => mt.DomainType.Name, mt => mt);
		}

		ILookup<Type, GuidAccessor> makeGuidAccessorsByType()
		{
			var allGuidAccessors = _model.MetaTypes.Select(mt => mt.collectGuidAccessors()).SelectMany(s => s);
			return allGuidAccessors.ToLookup(ga => ga.Type);
		}
	}
}
