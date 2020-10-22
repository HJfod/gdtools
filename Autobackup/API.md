In case you'd like to call the Autobackup exe from your own program, here are the parameters you need to provide in the .autobackup file.

File template:

```
param value
```

Example:

```
src C:/Users/Example/AppData/Local/GeometryDash
dest C:/Users/Example/Documents/Backups
create_on_gd_close 1
enabled 1
limit 5
```

| Param | Expected value | Description | Default | Required | Notes |
| :---- | -------------- | ----------- | ------- | :------: | ----- |
| src   | string         | GeometryDash directory (leave unset unless using a private server) | "" |   |   |
| dest | string | The directory new backups should be put in |   | ✓ |   |
| enabled | intbool (1 / 0) | Whether the autobackuper is enabled or not |   | ✓ |   |
| rate | unsigned int | How often backups should be made (in minutes) |   | ✓* | 1 |
| limit | unsigned int | Backup limit |   | ✓ |   |
| lastbackup | unsigned int | Last time a backup was made | 0 | ✓* | 1 |
| create_on_gd_close | intbool (1 / 0) | Whether the autobackuper should stay open in the background and make a backup when GD is closed | 0 |   |   |
| gd_check_rate | unsigned int | How often autobackup should check if GD is open | 10 |   | 2 |
| gd_check_length | unsigned int | On how many gd_check_rate timed checks does GD have to be open before making a backup | 3 |   | 2 |
| debug | intbool (1 / 0) | Whether the program should give extra command line info | 0 |   |   |
| compress | intbool (1 / 0) | Whether the backups should be compressed or not | 0 |   | 1 |
| compress_ext | string | The extention of the compressed files | .gbb |   | 1 |

Notes

| Note | Description |
| ---- | ----------- |
| * | Not if create_on_gd_close is 1 |
| 1 | Not currently implemented |
| 2 | Only set if create_on_gd_close is 1 |