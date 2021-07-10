<template>
  <div>
    <v-snackbar
      v-model="dialog.on"
      :bottom="dialog.y === 'bottom'"
      :left="dialog.x === 'left'"
      :color="
        dialog.type === 'success'
          ? 'success'
          : dialog.type === 'error'
          ? 'red'
          : 'info'
      "
      :multi-line="dialog.mode === 'multi-line'"
      :right="dialog.x === 'right'"
      :timeout="dialog.timeout"
      :top="dialog.y === 'top'"
      :vertical="dialog.mode === 'vertical'"
    >
      <div v-if="dialog.type === 'success'">
        <fa-icon icon="check"></fa-icon>
      </div>
      <p class="black--text text--lighten-2">{{ dialog.message }}</p>
      <v-btn color="pink" text @click="dialog.on = false">
        Close
      </v-btn>
    </v-snackbar>

    <v-container grid-list-xl>
      <v-expansion-panel v-model="panel" expand>
        <v-expansion-panel-content
          v-for="(folder, i) in folderFiles"
          :key="folder.id"
        >
          <div slot="header">
            {{ folder.folder }} ({{ folder.files.length }})
          </div>
          <v-list>
            <div v-if="folder.files.length === 0" class="text-xs-center">
              No files here
            </div>
            <template v-for="file in folder.files">
              <v-list-item>
                <v-list-item-content>
                  {{ file }}
                </v-list-item-content>
                <v-list-item-action v-if="isAdmin">
                  <v-btn icon @click="deleteItem(file, folder.folder)"
                    ><fa-icon size="lg" icon="window-close"
                  /></v-btn>
                </v-list-item-action>
              </v-list-item>
            </template>
          </v-list>
        </v-expansion-panel-content>
      </v-expansion-panel>
    </v-container>

    <v-container grid-list-xl v-show="files.length > 0">
      <div class="text-xs-center">Files being uploaded...</div>
      <v-list>
        <template v-for="(file, i) in files">
          <v-list-item>
            <v-list-item-content>
              {{ file.name }}
            </v-list-item-content>
          </v-list-item>
        </template>
      </v-list>
    </v-container>

    <v-form enctype="multipart/form-data">
      <v-card id="drop-area">
        <v-layout row wrap>
          <!--<v-flex xs12>
                    <div class="text-xs-center red--text text--accent-4 headline">Upload files here</div>
                </v-flex>-->

          <v-flex xs12>
            <v-select
              :items="paths"
              v-model="path"
              label="Hard Drive"
            ></v-select>
          </v-flex>

          <v-flex xs12 class="text-xs-center">
            <label id="upload-button" for="upload-files">UPLOAD FILES</label>
          </v-flex>
          <input
            ref="fUpload"
            type="file"
            name="upload-files"
            id="upload-files"
            class="file-upload"
            multiple
            @change="setFiles"
            hidden
          />
        </v-layout>
      </v-card>
    </v-form>
  </div>
</template>

<script>
import * as CONST from '../../constants'

let dropArea = null

export default {
  data() {
    return {
      paths: [],
      panel: [],
      path: '',
      files: [],
      folderFiles: [],
      role: CONST.Roles.Level.Default,
      c: CONST.Roles,
      dialog: {
        on: false,
        x: null,
        y: 'top',
        message: '',
        mode: 'multi-line',
        type: 'success',
        timeout: 5000,
      },
    }
  },
  created() {
    let role = this.$store.state.auth.role

    if (role === CONST.Roles.Name.Admin) {
      this.isAdmin = true
    } else if (role !== CONST.Roles.Name.Elevated) {
      this.$router.push({ name: 'start' })
    }

    //this.getData();
  },
  methods: {
    //async getData() {
    //    let failure = {
    //        folders: false,
    //        files: false
    //    }
    //    //let getFolders = service.getFolders().then(resp => {
    //    //    this.paths = resp.data
    //    //    if (this.paths.length > 0)
    //    //        this.path = this.paths[0]
    //    //}).catch(() => {
    //    //    this.$_console_log('Failed to get list of folders');
    //    //    failure.folders = true
    //    //});
    //    //let getFiles = service.getList().then(resp => {
    //    //    this.folderFiles = resp.data
    //    //}).catch(() => {
    //    //    this.$_console_log('Failed to get uploaded file directory contents');
    //    //    failure.files = true;
    //    //});
    //    await getFolders;
    //    await getFiles;
    //    if (failure.folders === true || failure.files === true) {
    //        this.dialog.on = true;
    //        this.dialog.type = 'error';
    //        this.dialog.timeout = 0;
    //        if (failure.folders === true && failure.files === true)
    //            this.dialog.message = `Failed to get folders and list. Check appsettings.`;
    //        else if (failure.folders === true)
    //            this.dialog.message = `Failed to get folders. Check appsettings.`;
    //        else if (failure.files === true)
    //            this.dialog.message = `Failed to get list. Check appsettings.`;
    //    }
    //},
    //setFiles(e) {
    //    this.$_console_log(e.target.files);
    //    for (let i = 0; i < e.target.files.length; i++) {
    //        this.files.push(e.target.files[i]);
    //    }
    //    this.uploadFiles();
    //},
    //async sendFile(file) {
    //    let formData = new FormData();
    //    formData.append("File", file);
    //    formData.append("Name", this.path)
    //    await service.uploadFile(formData).then(resp => {
    //        this.$_console_log("Successfully uploaded file");
    //        let fIndex = this.folderFiles.findIndex(x => x.folder === this.path);
    //        if (fIndex !== -1) {
    //            this.folderFiles[fIndex].files.push(file.name);
    //            this.dialog.on = true;
    //            this.dialog.message = `Successfully uploaded file ${file.name}`;
    //            this.dialog.type = 'success';
    //            this.dialog.timeout = 5000;
    //        }
    //    }).catch(() => {
    //        this.$_console_log("Error uploading files");
    //        this.dialog.on = true;
    //        this.dialog.message = `Failed uploading file ${file.name}`;
    //        this.dialog.type = 'error';
    //        this.dialog.timeout = 15000;
    //    }).then(resp => {
    //        let index = this.files.findIndex(x => x.name === file.name);
    //        this.$_console_log(`Found file at index: ${index}`);
    //        if (index !== -1)
    //            this.files.splice(index, 1);
    //    });
    //},
    ////sortFolder() {
    ////    let index = this.paths.findIndex(x => x.value === this.path);
    ////    this.$_console_log('');
    ////    //if (index > -1)
    ////    //  this.folderFiles[index].files.sort();
    ////},
    //async uploadFiles() {
    //    while (this.files.length > 0) {
    //        await this.sendFile(this.files[0]).then(resp => {
    //            this.$_console_log("File sent!");
    //        }).catch(() => this.$_console_log("Failed to upload file"));
    //    }
    //    this.$_console_log("Finished sending all files");
    //    //this.sortFolder();
    //    this.files = [];
    //    this.$refs.fUpload.value = '';
    //},
    //async deleteItem(file, folder) {
    //    if (!this.isAdmin)
    //        return this.$_console_log("You're not allowed to do that sir...");
    //    this.$_console_log(`Delete: ${file} from folder: ${folder}`);
    //    let fi = file;
    //    let fol = folder;
    //    await service.deleteFile(file, folder).then(resp => {
    //        this.$_console_log('Successfully deleted the file');
    //        let fIndex = this.folderFiles.findIndex(x => x.folder === fol);
    //        if (fIndex > -1) {
    //            let fiIndex = this.folderFiles[fIndex].files.indexOf(fi);
    //            this.folderFiles[fIndex].files.splice(fiIndex, 1);
    //        }
    //    }).catch(() => this.$_console_log('Error deleting the item in upload'));
    //},
  },
}
</script>

<style scoped>
#upload-button {
  border: 2px solid gray;
  border-radius: 7px;
  padding: 8px;
  margin: 8px;
  cursor: pointer;
}

#upload-button:hover {
  background-color: gray;
}

#drop-area {
  border: 2px dashed #ccc;
  border-radius: 35px;
  min-width: 120px;
  max-width: 240px;
  font-family: sans-serif;
  margin: 100px auto;
  padding: 20px;
}

/*#drop-area.highlight {
        border-color: purple;
    }*/

p {
  margin-top: 0;
}

.my-form {
  margin-bottom: 10px;
}

/*#gallery {
        margin-top: 10px;
    }

    #gallery img {
        width: 150px;
        margin-bottom: 10px;
        margin-right: 10px;
        vertical-align: middle;
    }*/

.button {
  display: inline-block;
  padding: 10px;
  background: #ccc;
  cursor: pointer;
  border-radius: 5px;
  border: 1px solid #ccc;
}

.button:hover {
  background: #ddd;
}

/*#fileElem {
        display: none;
    }*/
</style>
