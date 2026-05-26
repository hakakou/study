DROP TABLE IF EXISTS shifts, employees, locations CASCADE;

CREATE TABLE locations (
    location_id   SERIAL PRIMARY KEY,
    name          TEXT NOT NULL,
    city          TEXT NOT NULL
);

CREATE TABLE employees (
    employee_id   SERIAL PRIMARY KEY,
    name          TEXT NOT NULL,
    role          TEXT NOT NULL,        -- 'cook', 'server', 'manager'
    hourly_rate   NUMERIC(6,2) NOT NULL
);

CREATE TABLE shifts (
    shift_id      SERIAL PRIMARY KEY,
    location_id   INT NOT NULL REFERENCES locations(location_id),
    employee_id   INT NOT NULL REFERENCES employees(employee_id),
    shift_date    DATE NOT NULL,
    hours         NUMERIC(4,2) NOT NULL
);

INSERT INTO locations (name, city) VALUES
('Downtown Bistro',   'Seattle'),
('Harbor Grill',      'Seattle'),
('Airport Cafe',      'Seattle'),
('Mountain View',     'Bellevue');

INSERT INTO employees (name, role, hourly_rate) VALUES
('Anna Park',     'cook',    28.00),
('Brian Yu',      'server',  19.50),
('Cora Diaz',     'manager', 35.00),
('Devon Hill',    'cook',    27.00),
('Eli Brooks',    'server',  20.00),
('Faye Nguyen',   'server',  19.00);

-- Week of June 2-8, 2025 (Mon-Sun)
INSERT INTO shifts (location_id, employee_id, shift_date, hours) VALUES
-- Downtown Bistro: covered Mon, Wed, Fri, Sat, Sun
(1, 1, '2025-06-02', 8.0),
(1, 2, '2025-06-02', 6.0),
(1, 1, '2025-06-04', 8.0),
(1, 3, '2025-06-06', 7.5),
(1, 2, '2025-06-06', 6.0),
(1, 1, '2025-06-07', 8.0),
(1, 2, '2025-06-08', 5.0),
-- Harbor Grill: covered Tue, Thu, Sat
(2, 4, '2025-06-03', 8.0),
(2, 5, '2025-06-03', 6.5),
(2, 4, '2025-06-05', 8.0),
(2, 4, '2025-06-07', 8.0),
(2, 5, '2025-06-07', 7.0),
-- Airport Cafe: covered every day (it's an airport)
(3, 6, '2025-06-02', 8.0),
(3, 6, '2025-06-03', 8.0),
(3, 6, '2025-06-04', 8.0),
(3, 6, '2025-06-05', 8.0),
(3, 6, '2025-06-06', 8.0),
(3, 6, '2025-06-07', 8.0),
(3, 6, '2025-06-08', 8.0),
-- Mountain View: NO shifts at all this week (newly opened, not staffed yet)
-- (intentionally empty)
-- Plus one shift from the PREVIOUS week (should be excluded by date filter)
(1, 1, '2025-05-26', 8.0);