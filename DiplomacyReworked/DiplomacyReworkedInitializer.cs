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

        public DiplomacyReworkedInitializer()
        {
            BasicLoggingUtil logger = new BasicLoggingUtil();
            this.dataHub = new DataHub(logger);
            this.keepFiefBehaviour = new DiplomacyReworkedKeepFiefBehaviour(logger,this.dataHub);
            this.kingdomDiplomacyBehaviour = new DiplomacyReworkedKingdomDiplomacyBehaviour(logger, this.dataHub);
            this.menuBehaviour = new DiplomacyReworkedMenuBehaviour(logger,this.dataHub);
            this.cdManager = new DiplomacyReworkedCoolDownManagerBehaviour(logger, this.dataHub);
        }

    }
}
