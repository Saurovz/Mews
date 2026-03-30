import { useNavigate, useLocation } from "react-router-dom";

export default function SideMenu({ setShowSidebar }: { setShowSidebar: (value: boolean) => void }) {
  const navigate = useNavigate();
  const location = useLocation();

  const handleClick = (path: string) => {
    setShowSidebar(false);  // Hide the sidebar
    navigate(path);         // Navigate to route
  };

  const menuItems = [
    { title: "Home", icon: "/images/home-icon.svg", route: "/" },
    { title: "Taxations", icon: "/images/legal-icon.svg", route: "/taxations" },
    { title: "Legal Environments", icon: "/images/taxenv-icon.svg", route: "/legal-environments" },
    { title: "City Taxes", icon: "/images/city-icon.svg", route: "/city-taxes" },
  ];

  return (
    <div style={{ width: '270px' }} className="bg-white side-menu shadow">
      <aside className="p-4 space-y-2 font-semibold text-sm text-gray-800">
        {menuItems.map((item, index) => {
          const isActive = location.pathname === item.route;

          return (
            <div
              key={index}
              onClick={() => handleClick(item.route)}
              className={`flex items-center space-x-2 p-2 cursor-pointer rounded ${
                isActive ? 'bg-blue-100 text-blue-700' : 'hover:bg-gray-200'
              }`}
            >
              <img src={item.icon} alt={`${item.title} icon`} className="w-6 h-6" />
              <span className="sidemenutitle">{item.title}</span>
            </div>
          );
        })}
      </aside>
    </div>
  );
}
