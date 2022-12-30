<p align="center"><img src="assets/icon.svg" width="300" alt="Poncho Logo"></p>

<p align="center">
<img src="https://github.com/VallanDeMorty/poncho/actions/workflows/ci.yml/badge.svg">
</p>

A suggestion CLI, which helps with daily decision-making by providing a selected list of doings based on your schedule. Built for repeatable doings from 1 day to 1 month (possible to extend, but you probably need something else for such cases).

## Key Ideas

- Centric only around suggestions, it's totally up to you to decide what to do.
- Trusts you, so it doesn't track your doings by preserving you do your best, i.e. no need to check the boxes each F* day anymore.
- Built mainly to check the concept.

## How to use

### Create a journal

```bash
poncho journal
```

### Check the journal

```bash
poncho view
```

Example output:

```bash
Journal from December 28, 2022 to December 21, 2022

December 28, 2022
└── Doings
    ├── Read a Book(read-book)
    │   ├── Threshold: 3d
    │   └── Current: 2d
    └── Work the Plan Out (work-plan-out)
        ├── Threshold: 2d
        └── Current: 1d

December 27, 2022
├── Doings
│   ├── Read a Book(read-book)
│   │   ├── Threshold: 3d
│   │   └── Current: 1d
│   └── Work the Plan Out (work-plan-out)
│       ├── Threshold: 2d
│       └── Current: 2d
├── Commitments
│   └── Work the Plan Out
└── New Doings
    ├── Read a Book
    └── Work the Plan Out
```

### Plan the day

Use the `today` command to both plan the day and just initialize a new one.

```bash
poncho today
```

Example output:

```bash
December 27, 2022
├── Doings
│   ├── Read a Book(read-book)
│   │   ├── Threshold: 3d
│   │   └── Current: 1d
│   └── Work the Plan Out (work-plan-out)
│       ├── Threshold: 2d
│       └── Current: 2d
├── Commitments
│   └── Work the Plan Out
└── New Doings
    ├── Read a Book
    └── Work the Plan Out
```

### Add a doing

```bash
poncho add <name> <title> <threshold> -l "<last date when you did>"
```

The `l` flag is optional, however if you specify it you can describe it naturally like `21 days ago`.

### Remove a doing

```bash
poncho remove <name>
```

### Skip a doing

```bash
poncho skip <name> 
```

### Replace a doing

```bash
poncho replace <original-name> <new-name>
```

## Roadmap

- [x] Initialize the journal
- [x] Plan the day
  - [x] Print out the plan
- [x] Add doings
  - [ ] Print out the added doing
- [x] Remove doings
  - [ ] Print out the removed doing
- [x] Commit doings
  - [ ] Print out the committed doing
- [x] Skip doings
  - [ ] Print out the skipped doing
- [x] Replace doings
  - [ ] Print out the replaced doing
- [x] View the journal
- [x] Natural language date parsing

## License

MIT
