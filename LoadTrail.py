import pandas as pd
import sqlite3

DB_NAME = 'hikes.db'

def process_and_insert(csv_path, db_name):
    """Reads CSV and bulk inserts first two columns into SQLite."""
    conn = None 
    try:
        df = pd.read_csv(csv_path)

        if df.shape[1] < 2:
            print("Error: CSV must have at least two columns.")
            return

        print("Debug - DataFrame Head:")
        print(df.head())  

        df_insert = pd.DataFrame({
            'TrailID': 'AppalachianTrail',
            'TrailMile': df.iloc[:, 1], 
            'Name': df.iloc[:, 0]       
        })

        data_to_insert = list(df_insert.itertuples(index=False, name=None))

        if not data_to_insert:
            print("Error: No valid records processed for insertion.")
            return 

        conn = sqlite3.connect(db_name)
        cursor = conn.cursor()

        cursor.execute("""
            CREATE TABLE IF NOT EXISTS TrailPointsOfInterest (
                PointID INTEGER PRIMARY KEY,
                TrailID TEXT NOT NULL,
                TrailMile REAL,
                Name TEXT
            );
        """)
        cursor.execute("""
            CREATE INDEX IF NOT EXISTS idx_trail_points ON TrailPointsOfInterest (TrailID, TrailMile);
        """)

        # Insertion
        try:
            cursor.executemany("""
                INSERT INTO TrailPointsOfInterest (TrailID, TrailMile, Name) VALUES (?, ?, ?)
            """, data_to_insert)
            conn.commit()
            print(f"Success: {len(data_to_insert)} records inserted into {db_name}.")
        except sqlite3.Error as e:
            print(f"SQLite Error during insertion: {e}")
        
    except FileNotFoundError:
        print(f"Error: {csv_path} not found.") 
    except Exception as e:
        print(f"Critical error during operation: {e}")
    finally:
        if conn:
            conn.close()

if __name__ == "__main__":
    process_and_insert("AppalachianTrailLocations.csv", DB_NAME)
