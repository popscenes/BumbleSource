using System.Collections.Generic;
using Ninject;
using Ninject.Syntax;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace Website.Azure.Common.TableStorage
{
    public class UnitOfWorkForRepoJsonRepository : UnitOfWorkForRepoInterface
    {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly Stack<JsonRepository> _uowStack = new Stack<JsonRepository>();
        public UnitOfWorkForRepoJsonRepository(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public JsonRepository Repo { get { return _uowStack.Peek(); } }
        public GenericQueryServiceInterface CurrentQuery { get { return Repo; } }
        public GenericRepositoryInterface CurrentRepo { get { return Repo; } }


        #region Implementation of UnitOfWorkInterface

        public void Dispose()
        {
            if(_uowStack.Count > 0)
                End();
        }

        public bool Successful { get; private set; }
        public UnitOfWorkInterface Begin()
        {
            _uowStack.Push(_resolutionRoot.Get<JsonRepository>());
            Successful = false;
            return this;
        }

        public void End()
        {
            if (Repo == null) return;

            Successful = Repo.SaveChanges();
            _uowStack.Pop();     
        }

        #endregion

    }
}
