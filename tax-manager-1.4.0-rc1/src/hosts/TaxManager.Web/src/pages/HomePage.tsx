import { useState } from "react";
import { Route, Routes } from "react-router-dom";

import { SearchBar } from "../components/SearchBar/SearchBar.tsx";
import SideMenu from "../components/SideMenu/SideMenu.tsx";
import TaxationList from "./Taxations/TaxationList.tsx";
import TaxationDetail from "./Taxations/TaxationDetail.tsx";
import TaxationCreate from "./Taxations/TaxationCreate.tsx";
import LegalEnvironmentDetail from "./LegalEnvironment/LegalEnvironmentDetail.tsx";
import LegalEnvironmentCreate from "./LegalEnvironment/LegalEnvironmentCreate.tsx";
import LegalEnvironmentList from "./LegalEnvironment/LegalEnvironmentList.tsx";

export default function AppNavbar() {
  const [showSidebar, setShowSidebar] = useState(false);

  const toggleSidebar = () => {
    setShowSidebar(!showSidebar);
  };

    return (
        <div className="h-screen flex flex-col overflow-hidden">
            {/* Header */}
            <div className="bpBqjp sticky top-0 z-30 bg-[#f2f2f2] border-b border-gray-300">
                <div className="iAeqZk flex items-center justify-between w-full">
                    {/* Left section */}
                    <div className="flex items-center space-x-4">
                        <button className="p-2" onClick={toggleSidebar}>
                            <img
                                src="/images/menu-icon.svg"
                                alt="Menu"
                                className="w-6 h-6 cursor-pointer"
                            />
                        </button>
                        <span className="header">Mews Tax Manager</span>
                    </div>

                    {/* Search */}
                    <SearchBar />

                    {/* Right section */}
                    <div className="cjHtyS">
                        <div className="welcome">Welcome User!</div>
                        <img
                            src="/images/user.svg"
                            alt="user"
                            className="userIcon cursor-pointer pr-2"
                        />
                    </div>
                </div>
            </div>

            {/* Main content area with sidebar and scrollable content */}
            <div className="hgjqqI flex flex-1 overflow-hidden">
                {/* Sidebar */}
                <div className={`sidebar ${showSidebar ? "show" : "hide"}`}>
                    <SideMenu setShowSidebar={setShowSidebar} />
                </div>

                {/* Main scrollable area */}
                <div className={`p-4 main-content transition-all overflow-y-auto w-full duration-300 ${showSidebar ? "ml-64" : "ml-50"}`}>
                <Routes>
                    <Route path="/taxations" element={<TaxationList />} />
                    <Route path="/taxations/taxation-detail/:code" element={<TaxationDetail />} />
                    <Route path="/taxations/createNew" element={<TaxationCreate />} />
                    <Route path="/legal-environments" element={<LegalEnvironmentList />} />
                    <Route path="/legal-environments/legal-environment-detail/:code" element={<LegalEnvironmentDetail />} />
                    <Route path="/legal-environments/createNew" element={<LegalEnvironmentCreate />} />
                </Routes>
                </div>
            </div>

            {/* Footer */}
            <footer className="copyrightsec text-center text-sm py-2 bg-gray-100 border-t">
                &copy; {new Date().getFullYear()} Tax Manager. All rights reserved.
            </footer>
        </div>
    );
}
