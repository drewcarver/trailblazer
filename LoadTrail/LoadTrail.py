import os

import pandas as pd

try:
    from libsql_client import create_client_sync
except ImportError:  # pragma: no cover - runtime guidance path
    create_client_sync = None


TRAIL_NAME = "AppalachianTrail"
DB_CONNECTION_ENV = "DB_CONNECTION_STRING"
AUTH_TOKEN_ENV = "TURSO_AUTH_TOKEN"


def load_dotenv_if_present():
    """Loads KEY=VALUE pairs from the nearest .env file if present."""
    script_dir = os.path.dirname(os.path.abspath(__file__))
    candidates = [
        os.path.join(os.getcwd(), ".env"),
        os.path.join(script_dir, ".env"),
        os.path.join(os.path.dirname(script_dir), ".env"),
    ]

    dotenv_path = next((path for path in candidates if os.path.exists(path)), None)
    if not dotenv_path:
        return

    with open(dotenv_path, "r", encoding="utf-8") as env_file:
        for raw_line in env_file:
            line = raw_line.strip()
            if not line or line.startswith("#") or "=" not in line:
                continue

            key, value = line.split("=", 1)
            key = key.strip()
            value = value.strip().strip('"').strip("'")
            os.environ.setdefault(key, value)


def normalize_connection_settings(db_connection, auth_token=None):
    """Parses URL plus optional ;AuthToken=... style connection parts."""
    raw = (db_connection or "").strip()
    if not raw:
        return None, auth_token

    parts = [part.strip() for part in raw.split(";") if part.strip()]
    url = parts[0]

    resolved_token = auth_token
    for part in parts[1:]:
        key, sep, value = part.partition("=")
        if not sep:
            continue

        if key.strip().lower() == "authtoken" and not resolved_token:
            resolved_token = value.strip()

    return url, resolved_token


def candidate_urls(db_url):
    """Returns URL candidates to handle Turso protocol compatibility issues."""
    urls = [db_url]

    if db_url.startswith("libsql://"):
        urls.append("https://" + db_url[len("libsql://") :])

    return urls


def create_turso_client(db_url, auth_token):
    """Tries known URL forms; some environments reject websocket upgrades with HTTP 400."""
    last_error = None

    for url in candidate_urls(db_url):
        client = None
        try:
            client = create_client_sync(url=url, auth_token=auth_token)
            client.execute("SELECT 1")
            if url != db_url:
                print(f"Info: fallback connection succeeded using {url}.")
            return client
        except Exception as error:
            last_error = error
            if client:
                client.close()

    raise last_error


def process_and_insert(csv_path, db_url, auth_token=None):
    """Reads CSV and inserts first two columns into a Turso/libSQL database."""
    if create_client_sync is None:
        print("Error: missing dependency 'libsql-client'. Install with: pip install libsql-client")
        return

    client = None
    try:
        df = pd.read_csv(csv_path)

        if df.shape[1] < 2:
            print("Error: CSV must have at least two columns.")
            return

        print(df.head())

        miles = pd.to_numeric(df.iloc[:, 1], errors="coerce")
        names = df.iloc[:, 0].astype(str)
        valid = miles.notna()

        data_to_insert = [
            (TRAIL_NAME, float(mile), name)
            for name, mile in zip(names[valid], miles[valid])
        ]

        if not data_to_insert:
            print("Error: No valid records processed for insertion.")
            return

        skipped = int((~valid).sum())
        if skipped > 0:
            print(f"Warning: Skipped {skipped} rows with invalid TrailMile values.")

        client = create_turso_client(db_url, auth_token)

        client.execute(
            """
            CREATE TABLE IF NOT EXISTS TrailPointsOfInterest (
                ID INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                TrailName TEXT NOT NULL,
                TrailMile REAL NOT NULL
            );
            """
        )
        client.execute(
            """
            CREATE INDEX IF NOT EXISTS idx_trail_points
            ON TrailPointsOfInterest (TrailName, TrailMile);
            """
        )

        # Turso Python client does not expose sqlite3-style executemany; insert row-by-row.
        for row in data_to_insert:
            client.execute(
                "INSERT INTO TrailPointsOfInterest (TrailName, TrailMile, Name) VALUES (?, ?, ?)",
                row,
            )

        print(f"Success: {len(data_to_insert)} records inserted into Turso.")

    except FileNotFoundError:
        print(f"Error: {csv_path} not found.")
    except Exception as error:
        print(f"Critical error during operation: {error}")
    finally:
        if client:
            client.close()


if __name__ == "__main__":
    load_dotenv_if_present()
    raw_db_connection = os.getenv(DB_CONNECTION_ENV)
    env_auth_token = os.getenv(AUTH_TOKEN_ENV)
    db_connection, auth_token = normalize_connection_settings(raw_db_connection, env_auth_token)
    script_dir = os.path.dirname(os.path.abspath(__file__))
    csv_path = os.path.join(script_dir, "AppalachianTrailLocations.csv")

    if not db_connection:
        print(f"Error: {DB_CONNECTION_ENV} is not set.")
    else:
        process_and_insert(csv_path, db_connection, auth_token)
