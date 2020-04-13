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
            this.dataHub = new DataHub();
            this.keepFiefBehaviour = new DiplomacyReworkedKeepFiefBehaviour();
            this.kingdomDiplomacyBehaviour = new DiplomacyReworkedKingdomDiplomacyBehaviour();
            this.menuBehaviour = new DiplomacyReworkedMenuBehaviour();
            this.keepFiefBehaviour.currentHub = this.dataHub;
            this.kingdomDiplomacyBehaviour.currentHub = this.dataHub;
            this.menuBehaviour.currentHub = this.dataHub;
        }

    }
}
