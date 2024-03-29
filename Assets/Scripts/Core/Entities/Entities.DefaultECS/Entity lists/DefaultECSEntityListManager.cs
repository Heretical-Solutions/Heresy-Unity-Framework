using System.Collections.Generic;

using HereticalSolutions.Repositories;

using DefaultEcs;

namespace HereticalSolutions.Entities
{
	public class DefaultECSEntityListManager
		: IEntityListManager<int, List<Entity>>
	{
		private int nextIDToAllocate;

		private Queue<int> freeListIDs;

		private IRepository<int, List<Entity>> entityListRepository;

		public DefaultECSEntityListManager(
			Queue<int> freeListIDs,
			IRepository<int, List<Entity>> entityListRepository)
		{
			this.freeListIDs = freeListIDs;

			this.entityListRepository = entityListRepository;

			nextIDToAllocate = 0;
		}

		public bool HasList(int listID)
		{
			return entityListRepository.Has(listID);
		}

		public List<Entity> GetList(int listID)
		{
			if (!entityListRepository.TryGet(
				listID,
				out var entityList))
			{
				return null;
			}

			return entityList;
		}

		public void CreateList(
			out int listID,
			out List<Entity> entityList)
		{
			if (freeListIDs.Count > 0)
			{
				listID = freeListIDs.Dequeue();
			}
			else
			{
				listID = nextIDToAllocate++;
			}

			entityList = new List<Entity>();

			entityListRepository.Add(
				listID,
				entityList);
		}

		public void RemoveList(int listID)
		{
			if (!entityListRepository.TryRemove(listID))
			{
				return;
			}

			freeListIDs.Enqueue(listID);
		}
	}
}