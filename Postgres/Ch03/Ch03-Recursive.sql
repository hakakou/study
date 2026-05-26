DROP SCHEMA IF EXISTS exercise_recursive CASCADE;
CREATE SCHEMA exercise_recursive;
SET search_path TO exercise_recursive;

CREATE TABLE employees (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    title VARCHAR(100) NOT NULL,
    manager_id INTEGER REFERENCES employees(id),
    salary NUMERIC(10, 2) NOT NULL,
    approval_limit NUMERIC(10, 2) NOT NULL,  -- max expense they can approve
    hired_at DATE NOT NULL
);

CREATE TABLE expense_reports (
    id SERIAL PRIMARY KEY,
    employee_id INTEGER NOT NULL REFERENCES employees(id),
    amount NUMERIC(10, 2) NOT NULL,
    description TEXT NOT NULL,
    submitted_at TIMESTAMP NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'pending'  -- pending, approved, rejected
);

-- CEO at the top, then VPs, Directors, Managers, ICs
INSERT INTO employees (name, title, manager_id, salary, approval_limit, hired_at) VALUES
    ('Alice Chen',      'CEO',                   NULL, 450000, 1000000, '2015-01-15'),
    ('Bob Martinez',    'VP Engineering',        1,    280000, 100000,  '2016-03-20'),
    ('Carla Singh',     'VP Sales',              1,    275000, 100000,  '2016-06-10'),
    ('David Kim',       'Director Engineering',  2,    195000, 25000,   '2017-09-01'),
    ('Eve Johnson',     'Director Platform',     2,    198000, 25000,   '2018-02-14'),
    ('Frank Liu',       'Director Sales East',   3,    185000, 25000,   '2017-11-30'),
    ('Grace Park',      'Eng Manager Backend',   4,    155000, 5000,    '2019-04-22'),
    ('Henry Bell',      'Eng Manager Frontend',  4,    152000, 5000,    '2019-08-15'),
    ('Iris Wong',       'Eng Manager DevOps',    5,    158000, 5000,    '2020-01-10'),
    ('Jack Reilly',     'Sales Manager',         6,    140000, 5000,    '2019-12-01'),
    ('Kate Olsen',      'Senior Engineer',       7,    135000, 500,     '2020-06-15'),
    ('Liam Doyle',      'Engineer II',           7,    105000, 500,     '2021-03-08'),
    ('Mia Nakamura',    'Senior Engineer',       8,    138000, 500,     '2020-09-22'),
    ('Nolan Pierce',    'Engineer I',            8,    88000,  0,       '2023-07-01'),
    ('Olivia Stern',    'SRE',                   9,    142000, 500,     '2021-05-17'),
    ('Pete Vargas',     'Account Executive',     10,   95000,  0,       '2022-02-28'),
    ('Quinn Hassan',    'Account Executive',     10,   97000,  0,       '2022-04-11');

INSERT INTO expense_reports (employee_id, amount, description, submitted_at) VALUES
    (14, 450,    'Conference travel',          '2026-05-10 09:15:00'),
    (12, 3200,   'Team offsite catering',      '2026-05-11 14:30:00'),
    (15, 8500,   'Server hardware emergency',  '2026-05-12 02:45:00'),
    (16, 1200,   'Client dinner Tokyo',        '2026-05-12 18:00:00'),
    (11, 28000,  'Annual security audit',      '2026-05-13 10:00:00'),
    (7,  4500,   'Team training course',       '2026-05-14 11:30:00'),
    (17, 75000,  'Industry sponsorship deal',  '2026-05-15 16:20:00');
	
	
	WITH RECURSIVE subtree(ancestor_id, id, depth, salary) AS (
  SELECT id, id, 0, salary FROM employees      -- each person is in their own subtree at depth 0
  UNION ALL
  select s.ancestor_id, e.id, s.depth+1, e.salary
  from employees e
  join subtree s on e.manager_id=s.id
)

--select * from subtree order by ancestor_id,id

select ancestor_id, sum(salary) from subtree
group by ancestor_id

---

set search_path to exercise_recursive;

with recursive q(parent, name, total, subordinates, level, seq) as (
 select manager_Id, name, 
     salary::numeric,
    (select count(*) from employees eq where eq.manager_id=employees.id  ) as heads,
	0::integer  as level, 
	array[id]
  from employees where manager_id is not  null

 UNION  
 
 select p.manager_Id,   p.name, 
   p.salary+total,
  (select count(*) from employees eq where eq.manager_id=p.id  ) as heads,
   level+1, 
    q.seq || p.id
	from employees p
  
 join q on p.id=q.parent
 
   
)
select name, sum(total) s, max(level), sum(subordinates) from q
group by name
order by s

--

set search_path to exercise_recursive;

WITH RECURSIVE subtree(ancestor_id, id, depth, salary) AS (
  SELECT id, id, 0, salary FROM employees    
  UNION ALL
  select s.ancestor_id, e.id, s.depth+1, e.salary
  from employees e
  join subtree s on e.manager_id=s.id
)

select name, ancestor_id, sum(s.salary), max(depth) as d from subtree s
join employees e on e.id=s.ancestor_id

group by name, ancestor_id
having max(depth) >0
order by ancestor_id

--

set search_path to exercise_recursive;

WITH RECURSIVE q(expense_id, submitter_id, amount, approval_limit, manager_id, index, apr) as (
    select id, employee_id, amount, 0::NUMERIC, 0::int, -1::int, 0::boolean from expense_reports
  UNION ALL
  select expense_id, submitter_id, amount, e.approval_limit, e.manager_id, index+1,
    e.approval_limit>=amount
  from employees e 
  join q on (e.id = q.manager_id or ( q.manager_id=0 and e.id=q.submitter_id))
   and q.apr = false
) 
select r.*,
(select manager_id from q where q.expense_id=r.id 
 order by index desc limit 1
 )
from expense_reports r
