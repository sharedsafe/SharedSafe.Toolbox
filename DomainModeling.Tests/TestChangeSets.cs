using System;
using DomainModeling.Storage;
using NUnit.Framework;
using Newtonsoft.Json;
using Toolbox;
using Toolbox.Geometry;

namespace DomainModeling
{
	[TestFixture]
	public class TestChangeSets
	{
		[Test]
		public void serializationTest()
		{
			var cs = new ChangeSet();
			cs.Events = new DomainEvent[2];
			cs.Events[0] = DomainEventRegistry.makeEvent(new NodeCreated());
			cs.Events[1] = DomainEventRegistry.makeEvent(new NodeMoved());

			Console.WriteLine(JsonConvert.SerializeObject(cs));
		}

		[Test]
		public void testPointSerialization()
		{
			var p = new Point(100, 10);
			var ser = JsonConvert.SerializeObject((object) p);
			var dser = JsonConvert.DeserializeObject<Point>(ser);
			Assert.That(dser.X, Is.EqualTo(p.X));
			Assert.That(dser.Y, Is.EqualTo(p.Y));
		}

		sealed class TestEvent : IDomainEvent
		{
		}

		[Test]
		public void testEventSerialization()
		{
			var ev = new DomainEvent
			{
				Event = new TestEvent()
			};

			var ser = JsonConvert.SerializeObject(ev);
			this.D(ser);
			var deser = JsonConvert.DeserializeObject<DomainEvent>(ser);

			Assert.That(deser != null);
			Assert.That(deser.Event.GetType(), Is.EqualTo(typeof(TestEvent)));
		}

		[Test]
		public void testDateTimeSerialization()
		{
			DateTime date = DateTime.UtcNow;


			var ev = ChangeSet.create(Guid.NewGuid(), date, new DomainEvent[0]);

			var ser = JsonConvert.SerializeObject(ev);
			this.D(ser);
			var deser = JsonConvert.DeserializeObject<ChangeSet>(ser);

			Assert.That(deser.Date, Is.EqualTo(date.Ticks));
		}
	}
}
