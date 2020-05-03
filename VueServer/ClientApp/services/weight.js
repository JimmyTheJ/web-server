import axios from '../axios'

const GetWeightUrl = `api/weight/list`;
const AddWeightUrl = `api/weight/add`;
const DeleteWeightUrl = `api/weight/delete`;

export default {
    getWeightList() {
        return axios.get(GetWeightUrl);
    },
    deleteWeight(id) {
        return axios.request({
            url: DeleteWeightUrl,
            method: 'delete',
            params: {
                id: id
            }
        });
    },
    addWeight(data) {
        return axios.post(AddWeightUrl, data);
    },
}
