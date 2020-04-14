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

        public DiplomacyReworkedInitializer()
        {
            BasicLoggingUtil logger = new BasicLoggingUtil();
            this.dataHub = new DataHub(logger);
            this.keepFiefBehaviour = new DiplomacyReworkedKeepFiefBehaviour(logger);
            this.kingdomDiplomacyBehaviour = new DiplomacyReworkedKingdomDiplomacyBehaviour(logger);
            this.menuBehaviour = new DiplomacyReworkedMenuBehaviour(logger);
            this.keepFiefBehaviour.hub = this.dataHub;
            this.kingdomDiplomacyBehaviour.hub = this.dataHub;
            this.menuBehaviour.hub = this.dataHub;
        }

    }
}
