using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomacyReworked
{
    class DiplomacyReworkedInitializer
    {
        public DataHub dataHub;
        public DiplomacyReworkedKeepFiefBehaviour keepFiefBehaviour;
        public DiplomacyReworkedKingdomDiplomacyBehaviour kingdomDiplomacyBehaviour;
        public DiplomacyReworkedMenuBehaviour menuBehaviour;
        public DiplomacyReworkedCoolDownManagerBehaviour cdManager;
        public DiplomacyReworkedDeclareWarBarterBehaviour deccingBehaviour;
        public BasicLoggingUtil logger;
        public DiplomacyReworkedInitializer()
        {
            this.logger = new BasicLoggingUtil();
            this.dataHub = new DataHub(logger);
            this.deccingBehaviour = new DiplomacyReworkedDeclareWarBarterBehaviour(this.dataHub, this.logger);
            this.keepFiefBehaviour = new DiplomacyReworkedKeepFiefBehaviour(this.logger,this.dataHub);
            this.kingdomDiplomacyBehaviour = new DiplomacyReworkedKingdomDiplomacyBehaviour(this.logger, this.dataHub);
            this.menuBehaviour = new DiplomacyReworkedMenuBehaviour(this.logger,this.dataHub);
            this.cdManager = new DiplomacyReworkedCoolDownManagerBehaviour(this.logger, this.dataHub);
        }

    }
}
