import store from '../store/index'

export default {
    methods: {
        $_module_userHasModule(module) {
            const modules = store.getters.getActiveModules;

            const obj = modules.find(x => x.id === module.name ||
                (typeof module.meta.relative !== 'undefined' && x.id === module.meta.relative)
            );

            if (typeof obj === 'undefined') {
                return false;
            }
            else {
                return true;
            }
        }
    }
}
