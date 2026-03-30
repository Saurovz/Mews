#!/bin/bash
set -e

# Handle environment variables
CONNECTION_STRING=${DB_CONNECTION_STRING:-""}
MIGRATION_ENABLED=${RUN_MIGRATIONS:-"false"}

# Run migrations if enabled
if [ "$MIGRATION_ENABLED" = "true" ]; then
    echo "Running database migrations..."
    chmod +x ./migration_bundle
    ./migration_bundle --connection "$CONNECTION_STRING"
    exit 0
else
    echo "Skipping database migrations..."
fi

# Start the application
exec dotnet Mews.Job.Scheduler.dll "$@"
