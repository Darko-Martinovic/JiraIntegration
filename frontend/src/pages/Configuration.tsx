import React, { useState, useEffect } from "react";
import { Eye, EyeOff, RefreshCw, CheckCircle, XCircle } from "lucide-react";

interface ConfigurationData {
  baseUrl: string;
  projectKey: string;
  email: string;
  maxResults: number;
  timeoutSeconds: number;
  hasApiToken: boolean;
  isValid: boolean;
}

const Configuration: React.FC = () => {
  const [config, setConfig] = useState<ConfigurationData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showApiToken, setShowApiToken] = useState(false);

  const fetchConfiguration = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(
        "http://localhost:5000/api/jira/configuration"
      );
      if (response.ok) {
        const data = await response.json();
        setConfig(data);
      } else {
        throw new Error(`HTTP ${response.status}`);
      }
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Failed to fetch configuration"
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchConfiguration();
  }, []);

  const getValidationIcon = (isValid: boolean) => {
    return isValid ? (
      <CheckCircle className="w-5 h-5 text-green-600" />
    ) : (
      <XCircle className="w-5 h-5 text-red-600" />
    );
  };

  const getValidationColor = (isValid: boolean) => {
    return isValid
      ? "border-green-300 bg-green-50"
      : "border-red-300 bg-red-50";
  };

  if (loading) {
    return (
      <div className="p-8">
        <div className="max-w-4xl mx-auto">
          <div className="flex items-center justify-center py-12">
            <RefreshCw className="w-8 h-8 animate-spin text-blue-600" />
            <span className="ml-3 text-lg text-gray-600">
              Loading configuration...
            </span>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-8">
        <div className="max-w-4xl mx-auto">
          <div className="bg-red-50 border border-red-200 rounded-lg p-6">
            <div className="flex items-center">
              <XCircle className="w-6 h-6 text-red-600" />
              <h3 className="ml-3 text-lg font-medium text-red-800">
                Configuration Error
              </h3>
            </div>
            <p className="mt-2 text-red-700">{error}</p>
            <button
              onClick={fetchConfiguration}
              className="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
            >
              <RefreshCw className="w-4 h-4 mr-2" />
              Retry
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="p-8">
      <div className="max-w-4xl mx-auto">
        {/* Page Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Configuration</h1>
          <p className="text-gray-600 mt-1">
            JIRA integration settings loaded from environment variables
          </p>
        </div>

        {/* Configuration Status Overview */}
        <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6 mb-8">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-xl font-semibold text-gray-900">
              Configuration Status
            </h2>
            <button
              onClick={fetchConfiguration}
              className="inline-flex items-center px-3 py-2 border border-gray-300 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              <RefreshCw className="w-4 h-4 mr-2" />
              Refresh
            </button>
          </div>

          <div
            className={`rounded-lg border p-4 ${getValidationColor(
              config?.isValid || false
            )}`}
          >
            <div className="flex items-center">
              {getValidationIcon(config?.isValid || false)}
              <span className="ml-3 text-sm font-medium text-gray-900">
                {config?.isValid
                  ? "Configuration is valid and complete"
                  : "Configuration has missing or invalid values"}
              </span>
            </div>
          </div>
        </div>

        {/* Configuration Details */}
        {config && (
          <div className="space-y-6">
            {/* JIRA Instance Configuration */}
            <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">
                JIRA Instance
              </h3>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Base URL
                  </label>
                  <div className="relative">
                    <input
                      type="text"
                      value={config.baseUrl}
                      readOnly
                      className="block w-full rounded-lg border-gray-300 bg-gray-50 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                    />
                    <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
                      {getValidationIcon(!!config.baseUrl)}
                    </div>
                  </div>
                  <p className="mt-1 text-xs text-gray-500">
                    Environment variable: JIRA_BASE_URL
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Project Key
                  </label>
                  <div className="relative">
                    <input
                      type="text"
                      value={config.projectKey}
                      readOnly
                      className="block w-full rounded-lg border-gray-300 bg-gray-50 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                    />
                    <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
                      {getValidationIcon(!!config.projectKey)}
                    </div>
                  </div>
                  <p className="mt-1 text-xs text-gray-500">
                    Environment variable: JIRA_PROJECT_KEY
                  </p>
                </div>
              </div>
            </div>

            {/* Authentication Configuration */}
            <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">
                Authentication
              </h3>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Email Address
                  </label>
                  <div className="relative">
                    <input
                      type="email"
                      value={config.email}
                      readOnly
                      className="block w-full rounded-lg border-gray-300 bg-gray-50 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                    />
                    <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
                      {getValidationIcon(!!config.email)}
                    </div>
                  </div>
                  <p className="mt-1 text-xs text-gray-500">
                    Environment variable: JIRA_USER_EMAIL
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    API Token
                  </label>
                  <div className="relative">
                    <input
                      type={showApiToken ? "text" : "password"}
                      value={
                        config.hasApiToken
                          ? "••••••••••••••••••••••••••••••••••••••••"
                          : "Not configured"
                      }
                      readOnly
                      className="block w-full rounded-lg border-gray-300 bg-gray-50 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                    />
                    <div className="absolute inset-y-0 right-0 flex items-center space-x-2 pr-3">
                      {config.hasApiToken && (
                        <button
                          type="button"
                          onClick={() => setShowApiToken(!showApiToken)}
                          className="text-gray-400 hover:text-gray-600"
                        >
                          {showApiToken ? (
                            <EyeOff className="w-4 h-4" />
                          ) : (
                            <Eye className="w-4 h-4" />
                          )}
                        </button>
                      )}
                      {getValidationIcon(config.hasApiToken)}
                    </div>
                  </div>
                  <p className="mt-1 text-xs text-gray-500">
                    Environment variable: JIRA_API_TOKEN
                  </p>
                </div>
              </div>
            </div>

            {/* Performance Configuration */}
            <div className="bg-white rounded-lg shadow-md border border-gray-200 p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">
                Performance Settings
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Max Results
                  </label>
                  <input
                    type="number"
                    value={config.maxResults}
                    readOnly
                    className="block w-full rounded-lg border-gray-300 bg-gray-50 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  />
                  <p className="mt-1 text-xs text-gray-500">
                    Maximum number of results returned in queries
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Timeout (seconds)
                  </label>
                  <input
                    type="number"
                    value={config.timeoutSeconds}
                    readOnly
                    className="block w-full rounded-lg border-gray-300 bg-gray-50 shadow-sm focus:border-blue-500 focus:ring-blue-500"
                  />
                  <p className="mt-1 text-xs text-gray-500">
                    HTTP request timeout duration
                  </p>
                </div>
              </div>
            </div>

            {/* Configuration Instructions */}
            <div className="bg-blue-50 border border-blue-200 rounded-lg p-6">
              <h3 className="text-lg font-semibold text-blue-900 mb-4">
                Configuration Instructions
              </h3>
              <div className="space-y-3 text-sm text-blue-800">
                <p>
                  <strong>To update these settings:</strong>
                </p>
                <ol className="list-decimal list-inside space-y-1 ml-4">
                  <li>
                    Modify the corresponding environment variables in your{" "}
                    <code className="bg-blue-100 px-1 rounded">.env</code> file
                  </li>
                  <li>Restart the application for changes to take effect</li>
                  <li>Use the refresh button to reload the configuration</li>
                </ol>
                <p className="mt-4">
                  <strong>Required environment variables:</strong>
                </p>
                <ul className="list-disc list-inside space-y-1 ml-4">
                  <li>
                    <code className="bg-blue-100 px-1 rounded">
                      JIRA_BASE_URL
                    </code>{" "}
                    - Your JIRA instance URL
                  </li>
                  <li>
                    <code className="bg-blue-100 px-1 rounded">
                      JIRA_USER_EMAIL
                    </code>{" "}
                    - Your JIRA account email
                  </li>
                  <li>
                    <code className="bg-blue-100 px-1 rounded">
                      JIRA_API_TOKEN
                    </code>{" "}
                    - Your JIRA API token
                  </li>
                  <li>
                    <code className="bg-blue-100 px-1 rounded">
                      JIRA_PROJECT_KEY
                    </code>{" "}
                    - Default project key
                  </li>
                </ul>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Configuration;
