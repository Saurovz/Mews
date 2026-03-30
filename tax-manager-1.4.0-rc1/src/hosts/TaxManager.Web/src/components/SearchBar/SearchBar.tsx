import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { FaSearch, FaTimes } from "react-icons/fa";

import { Search } from "../../types/Search";
import { globalSearch } from "../../services/SearchService";
import { useLoader } from "../Loader/loaderContext";

export const SearchBar = () => {
  const [query, setQuery] = useState({ code: "", name: "" });
  const [results, setResults] = useState<Search>({ taxations: [], legalEnvironments: [] });
  const [showDropdown, setShowDropdown] = useState(false);
  const [noResults, setNoResults] = useState(false);
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);
  const containerRef = useRef<HTMLDivElement | null>(null);
  const { setLoading } = useLoader();
  const navigate = useNavigate();

  const MIN_QUERY_LENGTH = 3;

  const isValidQuery = (code: string, name: string) =>
    code.length >= MIN_QUERY_LENGTH || name.length >= MIN_QUERY_LENGTH;

  const search = async (code: string, name: string) => {
    try {
      setLoading(true);
      const data = await globalSearch(code, name);
      setResults(data);
      setShowDropdown(true);
      setNoResults(false);
    } catch {
      setNoResults(true);
      setShowDropdown(true);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (field: "code" | "name", value: string) => {
    setQuery((prev) => ({ ...prev, [field]: value }));
  };

  const handleClear = () => {
    setQuery({ code: "", name: "" });
    setResults({ taxations: [], legalEnvironments: [] });
    setShowDropdown(false);
  };

  const handleResultClick = (type: "taxation" | "legal", code: string) => {
    setQuery({ code: "", name: "" });
    setShowDropdown(false);
    setLoading(false);

    navigate(
      type === "taxation"
        ? `/taxations/taxation-detail/${code}`
        : `/legal-environments/legal-environment-detail/${code}`
    );
  };

  const highlightMatch = (text: string, search: string) => {
    const index = text.toLowerCase().indexOf(search.toLowerCase());
    if (index === -1) return text;
    return (
      <>
        {text.slice(0, index)}
        <span style={{ backgroundColor: "#d6f5ff", fontWeight: "600" }}>
          {text.slice(index, index + search.length)}
        </span>
        {text.slice(index + search.length)}
      </>
    );
  };

  const handleRefocus = () => {
    const { code, name } = query;
    if (isValidQuery(code, name)) search(code, name);
  };

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setShowDropdown(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  useEffect(() => {
    const { code, name } = query;
  
    if (!isValidQuery(code, name)) {
      setShowDropdown(false);
      setResults({ taxations: [], legalEnvironments: [] });
      return;
    }
  
    if (timeoutRef.current) clearTimeout(timeoutRef.current);
  
    timeoutRef.current = setTimeout(() => {
      search(code, name);
    }, 300); // Debounce delay (ms)
  
    return () => {
      if (timeoutRef.current) clearTimeout(timeoutRef.current);
    };
  }, [query]);  

  return (
    <div ref={containerRef} className="relative w-full md:w-[600px]">
      <div className="flex items-center border border-gray-300 rounded-lg px-2 py-1 bg-white shadow-sm">
        <FaSearch className="text-gray-500 mr-2" />

        {(["code", "name"] as const).map((field) => (
          <label key={field} className="flex items-center mr-4 text-sm text-gray-700">
            {field.charAt(0).toUpperCase() + field.slice(1)}:
            <input
              type="text"
              value={query[field]}
              onChange={(e) => handleInputChange(field, e.target.value)}
              onFocus={handleRefocus}
              className={`ml-1 px-2 py-1 border-none outline-none w-24 md:w-32 text-sm ${query[field] ? "bg-[#d6f5ff]" : "focus:bg-[#d6f5ff]"}`}
            />
          </label>
        ))}

        {(query.code || query.name) && (
          <button onClick={handleClear} className="ml-auto bg-gray-200 text-gray-700 text-sm px-1 py-1 rounded">
            <FaTimes />
          </button>
        )}
      </div>

      {showDropdown && (
        <div className="absolute z-50 mt-2 w-full bg-white border rounded shadow max-h-64 overflow-y-auto">
          {noResults ? (
            <div className="px-4 py-3 text-sm text-gray-500 text-center">No results found</div>
          ) : (
            <>
              {results.taxations.length > 0 && (
                <div>
                  <div className="px-4 py-2 text-sm font-semibold text-gray-600 bg-gray-100">Taxations</div>
                  {results.taxations.map((item) => (
                    <div
                      key={`tax-${item.code}`}
                      onClick={() => handleResultClick("taxation", item.code)}
                      className="px-4 py-2 hover:bg-blue-100 cursor-pointer"
                    >
                      <div className="text-sm">
                        <b>Code:</b> {highlightMatch(item.code, query.code)}, <b>Name:</b> {highlightMatch(item.name, query.name)}
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {results.legalEnvironments.length > 0 && (
                <div>
                  <div className="px-4 py-2 text-sm font-semibold text-gray-600 bg-gray-100">Legal Environments</div>
                  {results.legalEnvironments.map((item) => (
                    <div
                      key={`legal-${item.code}`}
                      onClick={() => handleResultClick("legal", item.code)}
                      className="px-4 py-2 hover:bg-blue-100 cursor-pointer"
                    >
                      <div className="text-sm">
                        <b>Code:</b> {highlightMatch(item.code, query.code)}, <b>Name:</b> {highlightMatch(item.name, query.name)}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </>
          )}
        </div>
      )}
    </div>
  );
};